using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;
using Newtonsoft.Json;

class Program
{
	static List<Flight> flights = new List<Flight>();
        static List<Order> orders = new List<Order>(); // List of Order objects

	static void Main(string[] args)
	{
		bool isRunning = true;

		while (isRunning)
		{
			Console.WriteLine("\nSelect operation mode:");
			Console.WriteLine("1. Add flights");
			Console.WriteLine("2. Print flights");
			Console.WriteLine("3. Assign orders");
			Console.WriteLine("4. Print orders");
			Console.WriteLine("5. End program");

			string choice = Console.ReadLine();

			switch (choice)
			{
				case "1":
					AddFlights();
					break;

				case "2":
					PrintFlights();
					break;

				case "3":
					AssignOrders();
					break;

				case "4":
					PrintOrders();
					break;

				case "5":
					isRunning = false;
					Console.WriteLine("Ending program...");
					break;

				default:
					Console.WriteLine("Invalid option. Please try again.");
					break;
			}
		}
	}

	static void AddFlights()
	{
		Console.WriteLine("Enter '1' for standard input or '2' for file input:");

		string mode = Console.ReadLine();
		List<string> input = new List<string>();

		if (mode == "1")
		{
			Console.WriteLine("Enter flight data (empty line to finish):");
			string line;
			while (!string.IsNullOrWhiteSpace(line = Console.ReadLine()))
			{
				input.Add(line);
			}
		}
		else if (mode == "2")
		{
			Console.WriteLine("Enter the file path:");
			string filePath = Console.ReadLine();
			if (File.Exists(filePath))
			{
				input.AddRange(File.ReadAllLines(filePath));
			}
			else
			{
				Console.WriteLine("File not found.");
				return;
			}
		}

		flights.AddRange(ExtractFlights(input));
	}

	static void PrintFlights()
	{
		Console.WriteLine("\nExtracted Flights:");
		flights.ForEach(flight => flight.Print());
	}

	static void AssignOrders()
	{
		Console.WriteLine("Enter the JSON file path containing orders:");
		string filePath = Console.ReadLine();

		if (!File.Exists(filePath))
		{
			Console.WriteLine("File not found.");
			return;
		}

		string jsonContent = File.ReadAllText(filePath);
		Dictionary<string, Dictionary<string, string>> rawOrders = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(jsonContent);

		if (rawOrders == null)
		{
			Console.WriteLine("Invalid JSON format.");
			return;
		}

		orders.Clear();
		foreach (var order in rawOrders)
		{
			orders.Add(new Order { OrderId = order.Key, Destination = order.Value["destination"], AssignedFlight = null });
		}

		// Assign orders to flights
		foreach (var order in orders)
		{
			foreach (var flight in flights)
			{
				if (flight.ToCode == order.Destination && flight.Orders.Count < 20)
				{
					flight.Orders.Add(order.OrderId);
					order.AssignedFlight = flight;
					break;
				}
			}
		}

		Console.WriteLine("Orders have been assigned.");
	}

	static void PrintOrders()
	{
		Console.WriteLine("\nOrders Schedule:");
		foreach (var order in orders)
		{
			order.Print();
		}
	}

	static List<Flight> ExtractFlights(List<string> input)
	{
		List<Flight> flights = new List<Flight>();
		int currentDay = 0;
		Regex dayRegex = new Regex(@"Day (\d+):");
		Regex flightRegex = new Regex(@"Flight (\d+): .*?\((\w+)\) .*?\((\w+)\)");


		foreach (var line in input)
		{
			Match dayMatch = dayRegex.Match(line);
			if (dayMatch.Success)
			{
				currentDay = int.Parse(dayMatch.Groups[1].Value);
			}
			else
			{
				Match flightMatch = flightRegex.Match(line);
				if (flightMatch.Success)
				{
					string flightNumber = flightMatch.Groups[1].Value;
					string fromCode = flightMatch.Groups[2].Value;
					string toCode = flightMatch.Groups[3].Value;

					Flight flight = new Flight
					{
						FlightNumber = flightNumber,
							     FromCode = fromCode,
							     ToCode = toCode,
							     Day = currentDay
					};
					flights.Add(flight);
				}
			}
		}

		return flights;
	}
}

public class Flight
{
	public string FlightNumber { get; set; }
	public string FromCode { get; set; }
	public string ToCode { get; set; }
	public int Day { get; set; }
	public List<string> Orders { get; set; } = new List<string>(); 

	public void Print()
	{
		Console.WriteLine($"Flight {FlightNumber}: {FromCode} to {ToCode} Day: {Day}");
	}
}

public class Order
{
	public string OrderId { get; set; }
	public string Destination { get; set; }
	public Flight AssignedFlight { get; set; } // Nullable

	public void Print()
	{
		if (AssignedFlight != null)
		{
			Console.WriteLine($"Order: {OrderId}, FlightNumber: {AssignedFlight.FlightNumber}, Departure: YUL, Arrival: {Destination}, Day: {AssignedFlight.Day}");
		}
		else
		{
			Console.WriteLine($"Order: {OrderId}, FlightNumber: not scheduled");
		}
	}
}

