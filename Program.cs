
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

namespace ParserASU
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var url = "http://m.raspisanie.asu.edu.ru//student/faculty";
            HttpClient httpClient = new HttpClient();
            var result = await httpClient.PostAsync(url, new StringContent(""));
            var resultContent = await result.Content.ReadAsStringAsync();
            var timeTable = JsonSerializer.Deserialize<Facul>("{\"q\":"+ resultContent + "}");
            foreach (MyItem i in timeTable.q)
            {
                Console.WriteLine(i.id + " " + i.name);
                MultipartFormDataContent form = new MultipartFormDataContent();
                form.Add(new StringContent(i.id), "id_spec");
                HttpResponseMessage response = await httpClient.PostAsync("http://m.raspisanie.asu.edu.ru//student/specialty", form);
                var responseContent = await response.Content.ReadAsStringAsync();
                var speciality = JsonSerializer.Deserialize<Facul>("{\"q\":" + responseContent + "}");
                Console.WriteLine("Специальности");
                foreach (MyItem j in speciality.q)
                {
                    
                    Console.WriteLine(j.id + " " + j.name);
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
                                if (arrGroup.Length%3 == 0)
                                {
                                    if (u.Length == 6)
                                    {
                                        Console.WriteLine(u);
                                        form = new MultipartFormDataContent();
                                        HttpResponseMessage responseTimeTable = await httpClient.PostAsync("http://m.raspisanie.asu.edu.ru/student/" + u, form);
                                        var responseContentTimeTable = await responseTimeTable.Content.ReadAsStringAsync();
                                        responseContentTimeTable = Regex.Replace(responseContentTimeTable, @"\\u([\da-f]{4})", m => ((char)Convert.ToInt32(m.Groups[1].Value, 16)).ToString());
                                        List<string> hrefTags = new List<string>();

                                        var parser = new HtmlParser();
                                        var document = parser.ParseDocument(responseContentTimeTable);
                                        Console.WriteLine(1111111111111111);
                                        var els = document.QuerySelectorAll("div.vot_den");

                                        foreach (var e in els)
                                        {
                                            Console.WriteLine(e.InnerHtml);
                                            Console.WriteLine(22);
                                            var elsDen = e.QuerySelectorAll("div.dennedeli");

                                            foreach (var ee in elsDen)
                                            {
                                                Console.WriteLine(ee.InnerHtml);

                                            }
                                        }
                                    }                                    
                                }
                                else
                                {
                                    Console.WriteLine(u);
                                    form = new MultipartFormDataContent();
                                    HttpResponseMessage responseTimeTable = await httpClient.PostAsync("http://m.raspisanie.asu.edu.ru/student/" + u, form);
                                    var responseContentTimeTable = await responseTimeTable.Content.ReadAsStringAsync();
                                    responseContentTimeTable = Regex.Replace(responseContentTimeTable, @"\\u([\da-f]{4})", m => ((char)Convert.ToInt32(m.Groups[1].Value, 16)).ToString());
                                    List<string> hrefTags = new List<string>();

                                    var parser = new HtmlParser();
                                    var document = parser.ParseDocument(responseContentTimeTable);
                                    Console.WriteLine(1111111111111111);
                                    var els = document.QuerySelectorAll("div.vot_den");

                                    foreach (var e in els)
                                    {
                                        Console.WriteLine(e.InnerHtml);
                                        Console.WriteLine(22);
                                        var elsDen = e.QuerySelectorAll("div.dennedeli");

                                        foreach (var ee in elsDen)
                                        {
                                            Console.WriteLine(ee.InnerHtml);

                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
