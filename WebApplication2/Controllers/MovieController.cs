using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication2.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WebApplication2.Controllers
{
    public class MovieController : Controller
    {
        private readonly MovieContext _context;
        IWebHostEnvironment _appEnvironment;

        public MovieController(MovieContext context, IWebHostEnvironment appEnvironment)
        {
            _context = context;
            _appEnvironment = appEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            List<Movie> movies = await _context.Movies.ToListAsync();  
            return View(movies); 
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movies.FirstOrDefaultAsync(m => m.Id == id);
            if (movie == null)
            {
                return NotFound();
            }

            return View(movie);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var currentMovie = await _context.Movies.FindAsync(id);
            if (currentMovie == null) return NotFound();

            return View("~/Views/Movie/Edit.cshtml", currentMovie);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var movie = await _context.Movies.FindAsync(id);
            if (movie == null) return NotFound();

            return View("~/Views/Movie/Delete.cshtml", movie);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var movie = await _context.Movies.FindAsync(id);
            if (movie != null)
            {
                if (!string.IsNullOrEmpty(movie.Poster)
                    && movie.Poster.StartsWith("/images/"))
                {
                    string oldFilePath = Path.Combine(_appEnvironment.WebRootPath, movie.Poster.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                _context.Movies.Remove(movie);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index), "Movie");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
        int id,
        [Bind("Id,Name,Director,Genre,Year,Description,Poster")] Movie changedMovie,
        IFormFile? uploaded_poster)
        {
            if (id != changedMovie.Id) return NotFound();

            ModelState.Remove("Poster");
            ModelState.Remove("uploaded_poster");
            var existingMovie = await _context.Movies.FindAsync(id);
            if (existingMovie == null) return NotFound();

            if (!ModelState.IsValid)
            {
                foreach (var entry in ModelState)
                {
                    foreach (var error in entry.Value.Errors)
                    {
                        Console.WriteLine($"Ошибка валидации: {entry.Key} - {error.ErrorMessage}");
                    }
                }

                changedMovie.Poster = existingMovie.Poster;
                return View("~/Views/Movie/Edit.cshtml", changedMovie);
            }

            if (uploaded_poster != null)
            {
                if (!string.IsNullOrEmpty(existingMovie.Poster))
                {
                    string oldFilePath = Path.Combine(_appEnvironment.WebRootPath, existingMovie.Poster.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }
                string fileName = Path.GetFileName(uploaded_poster.FileName);
                string newPath = "/images/" + fileName;
                string fullPath = Path.Combine(_appEnvironment.WebRootPath, "images", fileName);
                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await uploaded_poster.CopyToAsync(stream);
                }

                changedMovie.Poster = newPath;
            }
            else
            {
                changedMovie.Poster = existingMovie.Poster;
            }

            existingMovie.Name = changedMovie.Name;
            existingMovie.Director = changedMovie.Director;
            existingMovie.Genre = changedMovie.Genre;
            existingMovie.Year = changedMovie.Year;
            existingMovie.Description = changedMovie.Description;
            existingMovie.Poster = changedMovie.Poster;
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Movie");
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View("~/Views/Movie/Create.cshtml");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(1000000000)]
        public async Task<IActionResult> Create(
            [Bind("Name,Director,Genre,Year,Description")] Movie newMovie,
            IFormFile uploaded_poster)
        {
            ModelState.Remove("Poster");

            if (!ModelState.IsValid)
            {
                return View("~/Views/Movie/Create.cshtml", newMovie);
            }

            if (uploaded_poster != null)
            {
                string fileName = Path.GetFileName(uploaded_poster.FileName);
                string path = "/images/" + fileName;
                string fullPath = Path.Combine(_appEnvironment.WebRootPath, "images", fileName);
                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await uploaded_poster.CopyToAsync(stream);
                }
                newMovie.Poster = path;
            }
            _context.Movies.Add(newMovie);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Movie");
        }
    }
}
    