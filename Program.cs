using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Threading;

namespace ParserASU
{
    class Program
    {
        static void Main(string[] args)
        {
            IWebDriver driver = new ChromeDriver();
            driver.Url = @"http://raspisanie.asu.edu.ru/student"; 
            var links = driver.FindElements(By.XPath(
                @".//div[@id='divBig']/div[@id='divLittle']/div[@class='fucl_div']/form/table/tbody/tr/td/select[@id='facul']/option"));
            foreach (IWebElement link in links)
            {
                Console.WriteLine(link.Text + " - " + link.GetAttribute("value"));
            }
            Thread.Sleep(3000);
        }
    }
}
