namespace BlendoBot.Module.CurrencyConverter.API;

public record Result<T, E> {
	public readonly bool IsSuccessful;
	public readonly T? Success;
	public readonly E? Error;

	public Result(T t) {
		IsSuccessful = true;
		Success = t;
		Error = default;
	}
	public Result(E e) {
		IsSuccessful = false;
		Success = default;
		Error = e;
	}
}
