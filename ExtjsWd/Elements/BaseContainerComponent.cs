﻿using ExtjsWd.js;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.PageObjects;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace ExtjsWd.Elements
{
    public abstract class BaseContainerComponent
    {
        protected IWebDriver Driver;

        protected BaseContainerComponent(IWebDriver driver)
            : this(driver, (int)ScenarioFixture.DefaultTimeoutForElements.TotalSeconds)
        {
        }

        protected BaseContainerComponent(IWebDriver driver, int timeoutInSeconds)
        {
            TimeoutInSeconds = timeoutInSeconds;
            Driver = driver;
            SystemErrorDetectionText = "Systeem fout";

            PageFactory.InitElements(driver, this);
        }

        public int AjaxRequestsBusy
        {
            get { return int.Parse(EvalJS("return window.ajaxRequests").ToString()); }
        }

        public IWebElement NotificationError
        {
            get { return NotificationErrors.FirstOrDefault(); }
        }

        public IWebElement NotificationSuccess
        {
            get { return Driver.FindElement(By.CssSelector(".x-msg-success")); }
        }

        public IWebElement NotificationWarning
        {
            get { return Driver.FindElement(By.CssSelector(".x-msg-warning")); }
        }

        public string SystemErrorDetectionText { get; set; }

        protected int TimeoutInSeconds { get; set; }

        private bool DocumentDownloadButtonInFrameDisplayed
        {
            get
            {
                return Driver.FindElements(By.CssSelector(".wd-messagefactory-download-document-button")).Any();
            }
        }

        private IEnumerable<IWebElement> NotificationErrors
        {
            get { return Driver.FindElements(By.CssSelector(".x-msg-error")); }
        }

        public void DownloadDocumentFrameShouldBeVisible()
        {
            Assert.IsTrue(Driver.Wait(10).Until(x => DocumentDownloadButtonInFrameDisplayed));
        }

        public object EvalJS(string js)
        {
            return ((IJavaScriptExecutor)Driver).ExecuteScript(js);
        }

        public T ExpectOpened<T>() where T : BaseContainerComponent
        {
            return (T)Activator.CreateInstance(typeof(T),
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.CreateInstance | BindingFlags.Instance,
                null, new object[] { Driver }, CultureInfo.CurrentCulture);
        }

        public IList<string> NotificationErrorsAsText()
        {
            return NotificationErrors.Select(err => err.Text).ToList();
        }

        public bool ThrowExceptionIfSystemErrorOccured()
        {
            var errors = string.Join(",", NotificationErrorsAsText());
            if (errors.Contains(SystemErrorDetectionText))
            {
                throw new ArgumentException("Found unexpected system error message!");
            }
            return true;
        }

        public DefaultWait<IWebDriver> Wait(int timeoutInSeconds)
        {
            return Driver.GetWait(timeoutInSeconds);
        }

        [Obsolete("Please Use WaitUntil or Driver.Wait instead.")]
        public WebDriverWait Wait()
        {
            return Driver.Wait(TimeoutInSeconds);
        }

        public void WaitForNotificationErrorDisappeared()
        {
            JSCommands.CloseAllTooltips();
        }

        public void WaitForNotificationSuccessDisappeared()
        {
            JSCommands.CloseAllTooltips();
        }

        public bool WaitUntil(int secs, Func<IWebDriver, bool> cond)
        {
            return CreateWebDriverWait(secs).Until(drvr => ThrowExceptionIfSystemErrorOccured() && cond(drvr));
        }

        public IWebElement WaitUntil(int secs, Func<IWebDriver, IWebElement> cond)
        {
            return CreateWebDriverWait(secs).Until(drvr =>
            {
                ThrowExceptionIfSystemErrorOccured();
                return cond(drvr);
            });
        }

        public bool WaitUntil(Func<IWebDriver, bool> cond)
        {
            return WaitUntil(TimeoutInSeconds, cond);
        }

        public IWebElement WaitUntil(Func<IWebDriver, IWebElement> cond)
        {
            return WaitUntil(TimeoutInSeconds, cond);
        }

        public abstract void WaitUntilComponentLoaded();

        protected IWebElement FindButton(IWebElement container, string text)
        {
            return container.FindElements(By.CssSelector(".x-btn")).Single(e => e.Text.Contains(text));
        }

        private DefaultWait<IWebDriver> CreateWebDriverWait(int secs)
        {
            var wait = new DefaultWait<IWebDriver>(Driver)
            {
                PollingInterval = TimeSpan.FromMilliseconds(100),
                Timeout = new TimeSpan(0, 0, secs)
            };
            wait.IgnoreExceptionTypes(typeof(NotFoundException));
            return wait;
        }
    }
}