using FirstApplication.Models;
using FirstApplication.NewFolder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NToastNotify;

namespace FirstApplication.Controllers
{
    public class MoviesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IToastNotification _notify;
        public MoviesController(ApplicationDbContext context, IToastNotification notify)
        {
            _context = context;
            _notify = notify;

        }
        public async Task<IActionResult> Index()
        {
            var movies = await _context.Movies.OrderByDescending(m => m.Rate).ToListAsync();
            return View(movies);
        }
        public async Task<IActionResult> Create()
        {
            var Modelview = new MovieFormViewModel
            {
                Genres = await _context.Genres.OrderBy(m => m.Name).ToListAsync()
            };
            return View(Modelview);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MovieFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Genres = await _context.Genres.OrderBy(m => m.Name).ToListAsync();
                return View("Create", model);
            }
            var files = Request.Form.Files;
            if (!files.Any())
            {
                model.Genres = await _context.Genres.OrderBy(m => m.Name).ToListAsync();
                ModelState.AddModelError("Poster", "Please entre a poster film");
                return View("Create", model);
            }
            var poster = files.FirstOrDefault();
            var allowExtension = new List<String> { ".png", ".jpg" };
            if (!allowExtension.Contains(Path.GetExtension(poster.FileName).ToLower()))
            {
                model.Genres = await _context.Genres.OrderBy(m => m.Name).ToListAsync();
                ModelState.AddModelError("Poster", "fil accept just pnj and jpg forms");
                return View("Create", model);
            }
            if (poster.Length > 1048576)
            {
                model.Genres = await _context.Genres.OrderBy(m => m.Name).ToListAsync();
                ModelState.AddModelError("Poster", "fil can not be biger than 1 mg");
                return View("Create", model);
            }
            using var datastreme = new MemoryStream();
            await poster.CopyToAsync(datastreme);
            var movies = new Movie
            {
                Title = model.Title,
                GenreId = model.GenreId,
                Poster = datastreme.ToArray(),
                Rate = model.Rate,
                StorLing = model.StorLing,
                Year = model.Year

            };
            _context.Add(movies);
            _context.SaveChanges();
            _notify.AddSuccessToastMessage("Create success");
            return RedirectToAction("Index");
        }
        public async Task<IActionResult> Edit(int? Id)
        {
            if (Id == null)
                return BadRequest();
            var movie = await _context.Movies.FindAsync(Id);
            if (movie == null)
                return NotFound();

            var model = new MovieFormViewModel
            {
                Id = movie.Id,
                Title = movie.Title,
                Genres = await _context.Genres.OrderBy(m => m.Name).ToListAsync(),
                GenreId = movie.GenreId,
                Poster = movie.Poster,
                Rate = movie.Rate,
                StorLing = movie.StorLing,
                Year = movie.Year,

            };
            return View("Create", model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(MovieFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Genres = await _context.Genres.OrderBy(m => m.Name).ToListAsync();
                return View("Create", model);
            }

            var movie = await _context.Movies.FindAsync(model.Id);
            var files = Request.Form.Files;
            if (files.Any())
            {
                var poster = files.FirstOrDefault();
                var allowExtension = new List<String> { ".png", ".jpg" };
                if (!allowExtension.Contains(Path.GetExtension(poster.FileName).ToLower()))
                {
                    model.Genres = await _context.Genres.OrderBy(m => m.Name).ToListAsync();
                    ModelState.AddModelError("Poster", "fil accept just pnj and jpg forms");
                    return View("Create", model);
                }
                if (poster.Length > 1048576)
                {
                    model.Genres = await _context.Genres.OrderBy(m => m.Name).ToListAsync();
                    ModelState.AddModelError("Poster", "fil can not be biger than 1 mg");
                    return View("Create", model);
                }
                using var datastreme = new MemoryStream();
                await poster.CopyToAsync(datastreme);
                movie.Poster = datastreme.ToArray();
            }



            movie.Title = model.Title;
            movie.GenreId = model.GenreId;

            movie.Rate = model.Rate;
            movie.StorLing = model.StorLing;
            movie.Year = model.Year;
            _context.SaveChanges();
            _notify.AddSuccessToastMessage("Update success");
            return RedirectToAction("Index");
        }
        public async Task<IActionResult> Details(int? Id)
        {
            if (Id == null)
                return BadRequest();
            var movie = await _context.Movies.Include(m => m.Genre).SingleOrDefaultAsync(m => m.Id == Id);
            if (movie == null)
                return NotFound();

            return View("Details", movie);
        }
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return BadRequest();

            var movie = await _context.Movies.FindAsync(id);

            if (movie == null)
                return NotFound();

            _context.Movies.Remove(movie);
            _context.SaveChanges();

            return Ok();

        }
    }
    }
