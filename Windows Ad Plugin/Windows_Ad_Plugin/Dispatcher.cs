/*Copyright (c) 2014 [Michael MacDonald]

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Windows_Ad_Plugin
{
    /// <summary>
    /// Handles dispatching to the UI and App Threads
    /// </summary>
    public static class Dispatcher
    {
        // needs to be set via the app so we can invoke onto App Thread (see App.xaml.cs)
        public static Action<Action> InvokeOnAppThread
        { get; set; }

        // needs to be set via the app so we can invoke onto UI Thread (see App.xaml.cs)
        public static Action<Action> InvokeOnUIThread
        { get; set; }
    }
}
