﻿// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE.txt file in the project root for full license information.

namespace Xunit.Sdk
{
    using System;
    using System.ComponentModel;
    using Xunit.Abstractions;

    public class WinFormsAdapterUITestCase : UITestCase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WinFormsAdapterUITestCase"/> class.
        /// </summary>
        /// <inheritdoc cref="UITestCase(IMessageSink, TestMethodDisplay, ITestMethod, object[])"/>
        public WinFormsAdapterUITestCase(
            IMessageSink diagnosticMessageSink,
            TestMethodDisplay defaultMethodDisplay,
            ITestMethod testMethod,
            object[] testMethodArguments = null)
            : base(diagnosticMessageSink, defaultMethodDisplay, testMethod, testMethodArguments)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WinFormsAdapterUITestCase"/> class
        /// for deserialization.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Called by the de-serializer; should only be called by deriving classes for de-serialization purposes")]
        public WinFormsAdapterUITestCase()
        {
        }

        internal override SyncContextAdapter Adapter => WinFormsSynchronizationContextAdapter.Default;
    }
}
