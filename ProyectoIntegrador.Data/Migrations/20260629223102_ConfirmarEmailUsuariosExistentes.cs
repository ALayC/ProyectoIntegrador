using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProyectoIntegrador.Data.Migrations
{
    /// <inheritdoc />
    public partial class ConfirmarEmailUsuariosExistentes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Marcar como confirmados todos los usuarios existentes antes de
            // activar la validación de email obligatoria.
            migrationBuilder.Sql(
                "UPDATE [Usuarios] SET [EmailConfirmado] = 1 WHERE [EmailConfirmado] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "UPDATE [Usuarios] SET [EmailConfirmado] = 0");
        }
    }
}
