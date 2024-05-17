using Application.DTO;
using Application.Services;
using Microsoft.AspNetCore.Mvc;


namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AssociationController : ControllerBase
    {
        private readonly AssociationService _associationService;

        List<string> _errorMessages = new List<string>();

        public AssociationController(AssociationService associationService)
        {
            _associationService = associationService;
        }

        //POST: api/Association
        //To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<AssociationDTO>> PostAssociation(AssociationDTO associationDTO)
        {
            AssociationDTO assoDTO = await _associationService.AddFromRest(associationDTO, _errorMessages);
            
            if (assoDTO != null)
                return Accepted(assoDTO);
            else
                return BadRequest(_errorMessages);
        }
    }
}