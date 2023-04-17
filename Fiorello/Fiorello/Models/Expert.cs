﻿namespace Fiorello.Models
{
    public class Expert
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Image { get; set; }
        public Position Position { get; set; }
        public int PositionId { get; set; }
    }
}
