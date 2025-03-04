using System.ComponentModel.DataAnnotations;

namespace WebApplication2.Models
{
    public class Movie
    {
        public int Id { get; set; }  
        public string Name { get; set; }
        public string Director { get; set; }
        public string Genre { get; set; }
        public int Year { get; set; }
        public string Poster { get; set; }
        public string Description {  get; set; }
    }
}
