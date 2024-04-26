namespace Application.DTO;

using Domain.Model;

public class HolidayPeriodDTO
{
	//pretende-se não exteriorizar o id de persistência
	//public long Id { get; set; }

	// atenção: embora possa ser chave única, email não deve servir de chave primária para foreign keys; está assim para servir de exemplo.
	public DateOnly StartDate { get; set; }
	public DateOnly EndDate { get; set; }
	
	public HolidayPeriodDTO() {
	}

	public HolidayPeriodDTO(DateOnly startDate, DateOnly endDate)
	{
		//Id = id;
		StartDate = startDate;
		EndDate = endDate;
	}

	
}