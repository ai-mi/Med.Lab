namespace Med.Labs.Api.Controllers
{
	using Med.Labs.Application.Handlers;
	using Med.Labs.Application.Queries;
	using Med.Labs.Domain.Commands;
	using Microsoft.AspNetCore.Mvc;


	[ApiController]
	[Route("api/labs")]
	public class LaboratoryController : ControllerBase
	{
		private readonly AddLaboratoryResultHandler _add;
		private readonly UpdateLaboratoryResultHandler _update;
		private readonly DeleteLaboratoryResultHandler _delete;
		private readonly LaboratoryQueryService _queries;

		public LaboratoryController(
			AddLaboratoryResultHandler add,
			UpdateLaboratoryResultHandler update,
			DeleteLaboratoryResultHandler delete,
			LaboratoryQueryService queries)
		{
			_add = add;
			_update = update;
			_delete = delete;
			_queries = queries;
		}

		[HttpPost("{patientId:guid}")]
		public async Task<IActionResult> Add(Guid patientId, [FromBody] AddLabResultRequest req)
		{
			await _add.Handle(new AddLaboratoryResultCommand(
				patientId,
				req.TestType,
				req.Result,
				req.NormalMin,
				req.NormalMax,
				req.Comment
			));

			return Created();
		}

		[HttpPut("{patientId:guid}/{resultId:guid}")]
		public async Task<IActionResult> Update(Guid patientId, Guid resultId, [FromBody] UpdateLabResultRequest req)
		{
			await _update.Handle(new UpdateLaboratoryResultCommand(
				resultId,
				patientId,
				req.TestType,
				req.Result,
				req.NormalMin,
				req.NormalMax,
				req.Comment
			));

			return Ok();
		}

		[HttpDelete("{patientId:guid}/{resultId:guid}")]
		public async Task<IActionResult> Delete(Guid patientId, Guid resultId)
		{
			await _delete.Handle(new DeleteLaboratoryResultCommand(
				patientId,
				resultId
			));

			return Ok();
		}

		[HttpGet("{patientId:guid}")]
		public async Task<IActionResult> GetAll(Guid patientId)
			=> Ok(await _queries.GetAll(patientId));

		[HttpGet("{patientId:guid}/type/{type}")]
		public async Task<IActionResult> GetByType(Guid patientId, string type)
			=> Ok(await _queries.GetByTestType(patientId, type));

		[HttpGet("{patientId:guid}/range")]
		public async Task<IActionResult> GetByDate(Guid patientId, DateTime from, DateTime to)
			=> Ok(await _queries.GetByDateRange(patientId, from, to));
	}

	public record AddLabResultRequest(string TestType, double Result, double NormalMin, double NormalMax, string? Comment);
	public record UpdateLabResultRequest(string TestType, double Result, double NormalMin, double NormalMax, string? Comment);

}