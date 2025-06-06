﻿//-----------------------------------------------------------------------
// <copyright file="NSwagSettings.cs" company="NSwag">
//     Copyright (c) Rico Suter. All rights reserved.
// </copyright>
// <license>https://github.com/RicoSuter/NSwag/blob/master/LICENSE.md</license>
// <author>Rico Suter, mail@rsuter.com</author>
//-----------------------------------------------------------------------

namespace NSwag.Commands
{
    /// <summary>Enumeration of .NET runtimes where a document can be processed.</summary>
    public enum Runtime
    {
        /// <summary>Use default and do no checks.</summary>
        Default,

        /// <summary>Full .NET framework, x64.</summary>
        WinX64,

        /// <summary>Full .NET framework, x86.</summary>
        WinX86,

        /// <summary>.NET 8 app.</summary>
        Net80,

        /// <summary>.NET 9 app.</summary>
        Net90,

        /// <summary>Execute in the same process.</summary>
        Debug
    }
}
