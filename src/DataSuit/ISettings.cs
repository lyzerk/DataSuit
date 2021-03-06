﻿using System;
using System.Collections.Generic;
using System.Text;
using DataSuit.Interfaces;
using DataSuit.Providers;

namespace DataSuit
{
    public interface ISettings
    {
        (Enums.RelationshipMap Type, int Value) Relationship { get; set; }
        Dictionary<string, IDataProvider> Providers { get; }
        void AddProvider(string key, IDataProvider provider);
        void AddProvider(Dictionary<string, IDataProvider> prov);
        bool RemoveProvider(string key);
        string Export();
        void Import(string stringData);
        void ImportByJsonField(IJsonFieldSettings item);

    }
}
