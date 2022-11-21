using System;
namespace MinimalApi.Model
{
    public class Todo
    {
        public int id { get; set; }
        public string? Name { get; set; }
        public bool isComplete { get; set; }
    }
}

