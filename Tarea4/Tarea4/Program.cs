using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using NUnit.Framework;
using AventStack.ExtentReports;
using AventStack.ExtentReports.Reporter;

namespace AmazonAutomationTests
{
    public class AmazonTests
    {
        private IWebDriver driver;
        private WebDriverWait wait;
        private ExtentReports extent;
        private ExtentTest test;

        public static void Main(string[] args)
        {
            var amazonTest = new AmazonTests();
            amazonTest.Setup();
            try
            {
                amazonTest.TestAmazonFeatures();
            }
            finally
            {
                amazonTest.TearDown();
            }
        }

        public void Setup()
        {
            driver = new ChromeDriver(@"C:\SeleniumDriver\chromedriver-win64");
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(15));
            driver.Manage().Window.Maximize();



            extent = new ExtentReports();
            var sparkReporter = new ExtentSparkReporter(@"C:\Users\HP\source\repos\Tarea4\Tarea4_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".html");
            extent.AttachReporter(sparkReporter);
        }

        public void TestAmazonFeatures()
        {
            test = extent.CreateTest("Amazon Automation Test").Info("Inicio de la prueba");

            try
            {
                // 1. Buscar un producto 
                test.Log(AventStack.ExtentReports.Status.Info, "Buscando 'laptops' en Amazon.");
                driver.Navigate().GoToUrl("https://www.amazon.com/");
                TakeScreenshot("HomePage");

                IWebElement searchBox = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(By.Id("twotabsearchtextbox")));
                searchBox.SendKeys("laptops");
                searchBox.SendKeys(Keys.Enter);
                TakeScreenshot("SearchResults");

                Assert.That(driver.Url.Contains("s?k=laptops"), "No se redirigió correctamente a la página de resultados.");
                test.Pass("Se realizó la búsqueda correctamente.");

                // 2. Aplicar filtro
                test.Log(AventStack.ExtentReports.Status.Info, "Aplicando filtro para Dell.");
                IWebElement dellFilter = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementExists(By.XPath("//span[text()='Dell']/preceding-sibling::div")));
                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView(true);", dellFilter);
                dellFilter.Click();

                wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.CssSelector(".s-main-slot")));
                TakeScreenshot("FilterApplied");
                test.Pass("Filtro aplicado correctamente: Dell.");

                // 3. Ver los detalles del producto
                test.Log(AventStack.ExtentReports.Status.Info, "Abriendo detalles del primer producto.");
                IWebElement firstProduct = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(By.CssSelector(".s-main-slot .s-result-item h2 a")));
                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView(true);", firstProduct);
                firstProduct.Click();
                TakeScreenshot("ProductDetails");

                IWebElement productTitle = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.Id("productTitle")));
                Assert.That(productTitle.Displayed, "No se muestran los detalles del producto.");
                test.Pass("Se visualizaron los detalles del producto correctamente.");

                // 4. Agregar un producto al carrito
                test.Log(AventStack.ExtentReports.Status.Info, "Agregando producto al carrito.");
                IWebElement addToCartButton = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(By.Id("add-to-cart-button")));
                addToCartButton.Click();

                IWebElement cartConfirmation = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.Id("sw-gtc")));
                TakeScreenshot("ProductAddedToCart");

                Assert.That(cartConfirmation.Displayed, "El producto no se agregó correctamente al carrito.");
                test.Pass("El producto fue agregado al carrito exitosamente.");

                // 5. Volver a la página principal
                test.Log(AventStack.ExtentReports.Status.Info, "Volviendo a la página principal.");
                IWebElement homeLogo = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(By.Id("nav-logo-sprites")));
                homeLogo.Click();
                TakeScreenshot("BackToHome");

                Assert.That(driver.Url.Contains("amazon.com"), "No se redirigió correctamente a la página principal.");
                test.Pass("Se volvió a la página principal correctamente.");
            }
            catch (Exception ex)
            {
                test.Fail("La prueba falló: " + ex.Message);
                TakeScreenshot("TestFailure");
                throw;
            }
        }

        private void TakeScreenshot(string stepName)
        {
            try
            {
                var screenshot = ((ITakesScreenshot)driver).GetScreenshot();
                var filePath = $@"C:\Users\HP\source\repos\Tarea4\Screenshots\{stepName}_{DateTime.Now:yyyyMMdd_HHmmss}.png";
                screenshot.SaveAsFile(filePath);
                test.AddScreenCaptureFromPath(filePath);
            }
            catch (Exception ex)
            {
                test.Log(AventStack.ExtentReports.Status.Warning, "No se pudo tomar captura de pantalla: " + ex.Message);
            }
        }

        public void TearDown()
        {
            extent.Flush();
            driver.Quit();
        }
    }
}