// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE.txt file in the project root for full license information.

namespace Xunit.Sdk
{
    using System;
    using System.ComponentModel;
    using Xunit.Abstractions;

    public class WinFormsAdapterUITheoryTestCase : UITheoryTestCase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WinFormsAdapterUITheoryTestCase"/> class.
        /// </summary>
        /// <inheritdoc cref="UITheoryTestCase(IMessageSink, TestMethodDisplay, TestMethodDisplayOptions, ITestMethod)"/>
        public WinFormsAdapterUITheoryTestCase(
            IMessageSink diagnosticMessageSink,
            TestMethodDisplay defaultMethodDisplay,
            TestMethodDisplayOptions defaultMethodDisplayOptions,
            ITestMethod testMethod)
            : base(diagnosticMessageSink, defaultMethodDisplay, defaultMethodDisplayOptions, testMethod)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WinFormsAdapterUITheoryTestCase"/> class
        /// for deserialization.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Called by the de-serializer; should only be called by deriving classes for de-serialization purposes")]
        public WinFormsAdapterUITheoryTestCase()
        {
        }

        internal override SyncContextAdapter Adapter => WinFormsSynchronizationContextAdapter.Default;
    }
}
