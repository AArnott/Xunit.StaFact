// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE.txt file in the project root for full license information.

namespace Xunit.Sdk
{
    using System;
    using System.ComponentModel;
    using Xunit.Abstractions;

    public class WpfAdapterUITheoryTestCase : UITheoryTestCase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WpfAdapterUITheoryTestCase"/> class.
        /// </summary>
        /// <inheritdoc cref="UITheoryTestCase(IMessageSink, TestMethodDisplay, TestMethodDisplayOptions, ITestMethod)"/>
        public WpfAdapterUITheoryTestCase(
            IMessageSink diagnosticMessageSink,
            TestMethodDisplay defaultMethodDisplay,
            TestMethodDisplayOptions defaultMethodDisplayOptions,
            ITestMethod testMethod)
            : base(diagnosticMessageSink, defaultMethodDisplay, defaultMethodDisplayOptions, testMethod)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WpfAdapterUITheoryTestCase"/> class
        /// for deserialization.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Called by the de-serializer; should only be called by deriving classes for de-serialization purposes")]
        public WpfAdapterUITheoryTestCase()
        {
        }

        internal override SyncContextAdapter Adapter => DispatcherSynchronizationContextAdapter.Default;
    }
}
