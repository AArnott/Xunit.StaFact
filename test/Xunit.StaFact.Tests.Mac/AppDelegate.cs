// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;

namespace Xunit.StaFact.Tests.Mac;

[Register("AppDelegate")]
public class AppDelegate : NSApplicationDelegate
{
    public override void DidFinishLaunching(NSNotification notification)
        => ThreadPool.QueueUserWorkItem(
            o => _exit(
                Xunit.ConsoleClient.Program.Main(new[]
                {
                    typeof(AppDelegate).Assembly.Location,
                    "-appdomains",
                    "denied",
                })));

    [DllImport(ObjCRuntime.Constants.libcLibrary)]
#pragma warning disable SA1300 // Element should begin with upper-case letter
    private static extern void _exit(int exitCode);
#pragma warning restore SA1300 // Element should begin with upper-case letter
}
