﻿using System;

namespace RedCell.Diagnostics.Update.Demo
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            Log.Console = true;
            Console.WriteLine("Hello World");
            var updater = new Updater();
            updater.StartMonitoring();
        }
    }
}