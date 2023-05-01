using System;

namespace RudderStack.Unity
{
    public struct RSSourceConfig
    {
        public bool     isHosted { get; set; }
        public RSSource source   { get; set; }

        public struct RSSource
        {
            public string   id                 { get; set; }
            public string   name               { get; set; }
            public string   writeKey           { get; set; }
            public bool     enabled            { get; set; }
            public string   sourceDefinitionId { get; set; }
            public string   createdBy          { get; set; }
            public string   workspaceId        { get; set; }
            public bool     deleted            { get; set; }
            public bool     transient          { get; set; }
            public DateTime createdAt          { get; set; }
            public DateTime updateTime         { get; set; }
        }
    }
}