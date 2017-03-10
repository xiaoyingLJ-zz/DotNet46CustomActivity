// Copyright (c) Microsoft Corporation. All Rights Reserved.

using System;

namespace DotNet46CustomActivity
{
    [Serializable]
    class MyDotNetActivityContext
    {
        public string ConnectionString { get; set; }
        public string FolderPath { get; set; }
        public string FileName { get; set; }
    }
}