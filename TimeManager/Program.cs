using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace TimeManager
{
	class Program
	{
		static void Main(string[] args)
		{
			var allowedRate = TimeSpan.FromSeconds(1);
			var httpListener = new HttpListener();
			httpListener.Prefixes.Add("http://+:666/");
			var nextAllowedTimestamp = DateTime.UtcNow.Add(allowedRate);
			long requestsCount = 0;
			var locker = new object();
			httpListener.Start();
			while (true)
			{
				try
				{
					var context = httpListener.GetContext();
					Task.Run(() =>
					{
						try
						{
							try
							{
								if (context.Request.Url.AbsoluteUri.EndsWith("/ask"))
								{
									using (var w = new StreamWriter(context.Response.OutputStream))
									{
										var timeToSleep = TimeSpan.Zero;
										lock(locker)
										{
											var now = DateTime.UtcNow;
											if (now < nextAllowedTimestamp)
											{
												timeToSleep = nextAllowedTimestamp - now;
												nextAllowedTimestamp = nextAllowedTimestamp.Add(allowedRate);
												if (nextAllowedTimestamp > now + TimeSpan.FromSeconds(30))
													nextAllowedTimestamp = now + TimeSpan.FromSeconds(30);
											}
											else
												nextAllowedTimestamp = now.Add(allowedRate);
										}
										if (timeToSleep != TimeSpan.Zero)
										{
											w.WriteLine($"You was freezed for {timeToSleep}");
											Console.WriteLine($"Sleeping {timeToSleep}");
											Thread.Sleep(timeToSleep);
										}
										Interlocked.Increment(ref requestsCount);
										Console.WriteLine("Go. Requests: " + requestsCount);
										w.WriteLine("Go");
									}
								}
							}
							finally
							{
								context.Response.Close();
							}
						}
						catch (Exception e)
						{
							Console.WriteLine(e);
						}
					});
				}
				catch (Exception e)
				{
					Console.WriteLine(e);
				}
			}
		}
	}
}