﻿namespace API.Models
{
    public class PostObject2D
    {
        
        public string EnvironmentId{ get; set; }
        public string PrefabId { get; set; }
        public float PositionX { get; set; }
        public float PositionY { get; set; }
        public float ScaleX { get; set; }
        public float ScaleY { get; set; }
        public float RotationZ { get; set; }
        public int SortingLayer { get; set; }
    }
}
