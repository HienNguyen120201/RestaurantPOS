﻿namespace RestaurantPOS.Models
{
    public class FoodViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int UnitPrice { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public string ImageURL { get; set; }
        public InsertFoodViewModel insertFoodViewModel { get; set; }
    }
}
