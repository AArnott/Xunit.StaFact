// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE file in the project root for full license information.

using Xunit.StaFact.Tests.Mac;

NSApplication.Init();

// Need to manually set up AppDelegate due to lack of UI
NSApplication.SharedApplication.Delegate = new AppDelegate();

NSApplication.Main(args);
