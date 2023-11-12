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
using Azure.Storage.Blobs.Models;

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

        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
        public async Task<IActionResult> OnPostAsync(IFormFile FileUpload)
        {
            if (!ModelState.IsValid)
            {
                //Console.WriteLine("Prediction Created Successfully");
                return Page();
            }

            if (FileUpload != null && FileUpload.Length > 0)
            {
                Prediction.FileName = FileUpload.FileName;

                try
                {
                    var containerName = Prediction.Question == Question.Earth ? earthContainerName : computerContainerName;
                    var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                    await containerClient.CreateIfNotExistsAsync();

                    var blobClient = containerClient.GetBlobClient(Prediction.FileName);
                    using (var stream = FileUpload.OpenReadStream())
                    {
                        await blobClient.UploadAsync(stream, true);
                    }

                    Prediction.Url = blobClient.Uri.ToString();
                }
                catch (Exception ex)
                {
                    ex.ToString();
                }
            }

            _context.Predictions.Add(Prediction);
            await _context.SaveChangesAsync();
            //Console.WriteLine("Prediction Created Successfully");

            return RedirectToPage("./Index");
        }
    }
}
