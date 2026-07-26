/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 *
 * SPDX-License-Identifier: MPL-2.0
 */
namespace LLMCodeExporter.Core.Models
{
    public enum ProjectType
    {
        AutoDetect,
        CSharp,
        Python,
        JavaScript,
        TypeScript,
        Java,
        Go,
        Generic,
        WebApp,
        Hybrid
    }

    public enum Language
    {
        CSharp,
        Python,
        JavaScript,
        TypeScript,
        Java,
        Go,
        Ruby,
        PHP,
        Generic
    }
}