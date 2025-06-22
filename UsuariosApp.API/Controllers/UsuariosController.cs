using Microsoft.AspNetCore.Mvc;
using UsuariosApp.Application.Dtos;
using UsuariosApp.Application.InterfaceService;

namespace UsuariosApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly IUsuarioService _usuarioService;

        public UsuariosController(IUsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
        }

        [HttpPost("criar")]
        public async Task<IActionResult> CreateUsuarioAsync([FromBody] UsuarioRequestDto dto)
        {
            var result = await _usuarioService.CreateUsuarioAsync(dto);
            return StatusCode(201, new
            {
                message = "Usuario cadastrado com sucesso!",
                result

            });
        }

        [HttpGet("obterpor/{id}")]
        public async Task<IActionResult> GetUsuarioByIdAsync([FromRoute] Guid id)
        {
            var usuario = await _usuarioService.GetUsuarioByIdAsync(id);
            return Ok(usuario);
        }

        [HttpGet("consultartodos")]
        public async Task<IActionResult> GetAllUsuariosAsync()
        {
            var usuarios = await _usuarioService.GetAllUsuariosAsync();

            if (!usuarios.Any())
                return NoContent();

            return Ok(usuarios);
        }

        [HttpPut("atualizarconta/{id}")]
        public async Task<IActionResult> AtualizarContaAsync(
        [FromRoute] Guid id,
        [FromBody] UsuarioRequestDto dto)
        {
            var resultado = await _usuarioService.UpdateUsuarioAsync(dto, id);

            return Ok(new
            {
                message = "Email e/ou senha atualizados com sucesso!",
                resultado
            });
        }

        [HttpDelete("deletar/{id}")]
        public async Task<IActionResult> ExcluirUsuarioAsync([FromRoute] Guid id)
        {

            await _usuarioService.DeleteUsuarioAsync(id);

            return Ok(new
            {
                message = "Usuário excluído com sucesso!",
                idExcluido = id
            });
        }


    }
}
