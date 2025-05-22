using EF_Prescription_Manager.DTO;
using EF_Prescription_Manager.Exceptions;
using EF_Prescription_Manager.Services;
using Microsoft.AspNetCore.Mvc;

namespace EF_Prescription_Manager.Controllers;

[Route("api/prescriptions")]
[ApiController]
public class PrescriptionController : ControllerBase
{
    private readonly IPrescriptionService _prescriptionService;

    public PrescriptionController(IPrescriptionService prescriptionService)
    {
        _prescriptionService = prescriptionService;
    }

    [HttpPost]
    public async Task<IActionResult> CreatePrescription(
        [FromBody] PerscriptionRequestDto prescription)
    {
        try
        {
            await _prescriptionService.AddPrescription(prescription);
            return Created(string.Empty, null);
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (ConflictException ex)
        {
            return Conflict(ex.Message);
        }
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetPatientById(
        [FromRoute] int id)
    {
        try
        {
            var patient = await _prescriptionService.GetPatient(id);
            return Ok(patient);
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }
}