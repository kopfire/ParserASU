
using System;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.Json;
using System.Collections;
using System.Net;
using Newtonsoft.Json.Linq;
using HtmlAgilityPack;
using System.Net.Http.Headers;
using System.Linq;
using System.Text.RegularExpressions;
using AngleSharp.Html.Parser;
using AngleSharp.Dom;
using ParserASU.Models;

namespace ParserASU
{
    class Program
    {

        private static readonly CountriesService CountriesDB = new CountriesService();

        private static readonly CitiesService CitiesDB = new CitiesService();

        private static readonly UniversitiesService UniversitiesDB = new UniversitiesService();

        private static readonly FacultiesService FacultiesDB = new FacultiesService();

        private static readonly SpecialtiesService SpecialitiesDB = new SpecialtiesService();

        private static readonly TimeTablesService TimeTablesDB = new TimeTablesService();

        static async Task Main(string[] args)
        {
            Dictionary<string, int> days = new Dictionary<string, int>(6);
            days.Add("ПН", 1);
            days.Add("ВТ", 2);
            days.Add("СР", 3);
            days.Add("ЧТ", 4);
            days.Add("ПТ", 5);
            days.Add("СБ", 6);
            var url = "http://m.raspisanie.asu.edu.ru//student/faculty";
            HttpClient httpClient = new HttpClient();
            var result = await httpClient.PostAsync(url, new StringContent(""));
            var resultContent = await result.Content.ReadAsStringAsync();
            var timeTable = JsonSerializer.Deserialize<Facul>("{\"q\":"+ resultContent + "}");
            foreach (MyItem i in timeTable.q)
            {
                Console.WriteLine(i.id + " " + i.name);
                var idF = await FacultiesDB.Create(new Faculties { Name = i.name.Replace(",", ""), University = "617477bee3592a8c4fe4458f" });
                MultipartFormDataContent form = new MultipartFormDataContent();
                form.Add(new StringContent(i.id), "id_spec");
                HttpResponseMessage response = await httpClient.PostAsync("http://m.raspisanie.asu.edu.ru//student/specialty", form);
                var responseContent = await response.Content.ReadAsStringAsync();
                var speciality = JsonSerializer.Deserialize<Facul>("{\"q\":" + responseContent + "}");
                Console.WriteLine("Специальности");
                foreach (MyItem j in speciality.q)
                {
                    Console.WriteLine(j.id + " " + j.name);
                    var idS = await SpecialitiesDB.Create(new Specialties { Name = j.name.Replace(",", ""), Facylty = idF });
                    form = new MultipartFormDataContent();
                    form.Add(new StringContent(j.id), "val_spec");
                    HttpResponseMessage responseKurs = await httpClient.PostAsync("http://m.raspisanie.asu.edu.ru//student/kurs", form);
                    var responseContentKurs = await responseKurs.Content.ReadAsStringAsync();
                    var kurs = JsonSerializer.Deserialize<Facul>("{\"s\":" + responseContentKurs + "}");
                    Console.WriteLine("Курсы");
                    foreach (Kurs z in kurs.s)
                    {

                        Console.WriteLine(z.kurs);
                        form = new MultipartFormDataContent();
                        form.Add(new StringContent(j.id), "val_spec");
                        form.Add(new StringContent(z.kurs), "kurs");
                        HttpResponseMessage responseGroup = await httpClient.PostAsync("http://m.raspisanie.asu.edu.ru//student/grup", form);
                        var responseContentGroup = await responseGroup.Content.ReadAsStringAsync();
                        if (responseContentGroup.Length > 4)
                        {
                            responseContentGroup = Regex.Replace(responseContentGroup, @"\\u([\da-f]{4})", m => ((char)Convert.ToInt32(m.Groups[1].Value, 16)).ToString());
                            responseContentGroup = responseContentGroup.Replace("\"", "");
                            responseContentGroup = responseContentGroup.Substring(1, responseContentGroup.Length - 2);
                            var arrGroup = responseContentGroup.Split(",");
                            foreach (string u in arrGroup)
                            {
                                if (arrGroup.Length%3 != 0 || u.Length == 6)
                                {
                                    Console.WriteLine(u);
                                    var weeks = new List<Helpers.JSON.Week>();
                                    form = new MultipartFormDataContent();
                                    HttpResponseMessage responseTimeTable = await httpClient.PostAsync("http://m.raspisanie.asu.edu.ru/student/" + u, form);
                                    var responseContentTimeTable = await responseTimeTable.Content.ReadAsStringAsync();
                                    responseContentTimeTable = Regex.Replace(responseContentTimeTable, @"\\u([\da-f]{4})", m => ((char)Convert.ToInt32(m.Groups[1].Value, 16)).ToString());
                                    List<string> hrefTags = new List<string>();

                                    var parser = new HtmlParser();
                                    var document = parser.ParseDocument(responseContentTimeTable);
                                    var els = document.QuerySelectorAll("div.vot_den");

                                    var dayCh = new List<Helpers.JSON.Day>();
                                    var dayZn = new List<Helpers.JSON.Day>();

                                    foreach (var e in els)
                                    {
                                        var day = days[e.QuerySelectorAll("div.dennedeli")[0].InnerHtml];

                                        var elsDay = e.QuerySelectorAll("div.den-content");

                                        var lesCh = new List<Helpers.JSON.Lesson>();
                                        var lesZn = new List<Helpers.JSON.Lesson>();

                                        foreach (var ee in elsDay)
                                        {
                                            var para = (ee.QuerySelectorAll("div.npara")[0].InnerHtml)[0] - '0';
                                            var time = ee.QuerySelectorAll("div.time-para")[0].InnerHtml;

                                            var chisl = ee.QuerySelectorAll("div.td_style2_ch")[0];

                                            if (chisl.QuerySelectorAll("span.naz_disc").Length != 0)
                                            {
                                                lesCh.Add(new Helpers.JSON.Lesson
                                                {
                                                    Audience = chisl.QuerySelectorAll("span.segueAud").Length != 0
                                                    ? chisl.QuerySelectorAll("span.segueAud")[0].InnerHtml : "",
                                                    Name = chisl.QuerySelectorAll("span.naz_disc")[0].InnerHtml,
                                                    Teacher = chisl.QuerySelectorAll("a.segueTeacher").Length != 0
                                                    ? chisl.QuerySelectorAll("a.segueTeacher")[0].InnerHtml : "",
                                                    Time = time.Replace("<br>", "-"),
                                                    Number = para
                                                });
                                            }

                                            var znamen = ee.QuerySelectorAll("div.td_style2_zn")[0];

                                            if (znamen.QuerySelectorAll("span.naz_disc").Length != 0)
                                            {
                                                lesZn.Add(new Helpers.JSON.Lesson
                                                {
                                                    Audience = znamen.QuerySelectorAll("span.segueAud").Length != 0
                                                    ? znamen.QuerySelectorAll("span.segueAud")[0].InnerHtml : "",
                                                    Name = znamen.QuerySelectorAll("span.naz_disc")[0].InnerHtml,
                                                    Teacher = znamen.QuerySelectorAll("a.segueTeacher").Length != 0
                                                    ? znamen.QuerySelectorAll("a.segueTeacher")[0].InnerHtml : "",
                                                    Time = time,
                                                    Number = para
                                                });
                                            }
                                        }
                                      
                                        if (lesCh.Count != 0)
                                            dayCh.Add(new Helpers.JSON.Day { Lessons = lesCh, Number = day });
                                        if (lesZn.Count != 0)
                                            dayZn.Add(new Helpers.JSON.Day { Lessons = lesZn, Number = day });
                                    }
                                    weeks.Add(new Helpers.JSON.Week { Days = dayCh, Number = 1 });
                                    weeks.Add(new Helpers.JSON.Week { Days = dayZn, Number = 0 });
                                    await TimeTablesDB.Create(new TimeTables { Speciality = idS, Group = u, Students = new List<long>(), Weeks = weeks });
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}