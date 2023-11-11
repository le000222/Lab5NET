using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Lab5.Data;
using Lab5.Models;
using Azure.Storage.Blobs;

namespace Lab5.Pages.Predictions
{
    public class CreateModel : PageModel
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string earthContainerName = "earthimages";
        private readonly string computerContainerName = "computerimages";
        private readonly Lab5.Data.PredictionDataContext _context;

        public CreateModel(Lab5.Data.PredictionDataContext context, BlobServiceClient blobServiceClient)
        {
            _context = context;
            _blobServiceClient = blobServiceClient;
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        [BindProperty]
        public Models.Prediction Prediction { get; set; }

        [BindProperty]
        public IFormFile FileUpload { get; set; }


        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                Console.WriteLine("Prediction Not Created");
                foreach (var entry in ModelState)
                {
                    var key = entry.Key; // The model property name
                    var errors = entry.Value.Errors; // A collection of ModelError instances

                    foreach (var error in errors)
                    {
                        var errorMessage = error.ErrorMessage; // The error message
                        var exception = error.Exception; // The exception associated with the error, if any

                        // Log, display, or handle the error accordingly
                        Console.WriteLine($"Property: {key}, Error Message: {errorMessage}, Exception: {exception?.Message}");
                    }
                }
                return Page();
            }

            if (FileUpload != null && FileUpload.Length > 0)
            {
                Console.WriteLine("File: ", FileUpload);
                Prediction.FileName = FileUpload.FileName;

                try
                {
                    var containerName = Prediction.Question == Question.Earth ? earthContainerName : computerContainerName;
                    var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                    await containerClient.CreateIfNotExistsAsync();

                    var blobClient = containerClient.GetBlobClient(Prediction.FileName);
                    await blobClient.UploadAsync(FileUpload.OpenReadStream(), true);

                    Prediction.Url = blobClient.Uri.ToString();
                }
                catch (Exception ex)
                {
                    ex.ToString();
                }
            }

            _context.Predictions.Add(Prediction);
            await _context.SaveChangesAsync();
            Console.WriteLine("Prediction Created Successfully");

            return RedirectToPage("./Index");
        }
    }
}
