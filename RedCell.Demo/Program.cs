﻿using System;
using System.Threading;
using RedCell.Diagnostics.Update;

namespace RedCell.Demo
{
    static class Program
    {
        static void Main()
        {
            Log.Console = true;
            var updater = new Updater();
            updater.StartMonitoring();
            
            Console.WriteLine("Hello World");
            while(true)
            {
                Console.WriteLine("Live");
                Thread.Sleep(3000);
            }
        }
    }
}