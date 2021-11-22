using System.Collections.Generic;

namespace ParserASU.Helpers.JSON
{
    public class Day
    {
        public int Number { get; set; }

        public List<Lesson> Lessons { get; set; }
    }
}
