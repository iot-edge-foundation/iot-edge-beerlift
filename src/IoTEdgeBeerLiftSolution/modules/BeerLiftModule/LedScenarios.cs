namespace BeerLiftModule
{
    using System;
    using System.Threading.Tasks;
    using Iot.Device.Mcp23xxx;

    class LedScenarios
    {
        private static bool _ledsPlaying = false;

        public static async Task LitFlooded(Mcp23xxx mcp23xxxWrite, int interval, bool silentFlooding)
        {
            if (silentFlooding)
            {
                return;
            }

            try
            {
                while(_ledsPlaying)
                {
                    // let the previous light show end.
                    await Task.Delay(5);
                }

                _ledsPlaying = true;

                Mcp23x1x mcp23x1x = null;
                
                if (mcp23xxxWrite != null)
                {
                    mcp23x1x = mcp23xxxWrite as Mcp23x1x;
                }

                if (mcp23x1x == null)
                {
                    Console.WriteLine("LitFlooded: Unable to cast Mcp23017 Write GPIO.");   
                }
                else
                {
                    var sleep = 250;

                    var steps = interval / sleep;

                    for (var i = 0; i < steps; i++)
                    {
                        mcp23x1x.WriteByte(Register.GPIO, 255 , Port.PortA);
                        mcp23x1x.WriteByte(Register.GPIO, 255, Port.PortB);

                        mcp23x1x.WriteByte(Register.GPIO, 0 , Port.PortA);
                        mcp23x1x.WriteByte(Register.GPIO, 0, Port.PortB);

                        await Task.Delay(sleep);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error when LitFlooded: {ex.Message}");
            }
            finally
            {
                _ledsPlaying = false;
            }
        }

        public static async Task LitAllEmptySpots(Mcp23xxx mcp23xxxWrite, byte lastDataPortA, byte lastDataPortB)
        {
            try
            {
                while(_ledsPlaying)
                {
                    // let the previous light show end.
                    await Task.Delay(5);
                }

                _ledsPlaying = true;

                Mcp23x1x mcp23x1x = null;
                
                if (mcp23xxxWrite != null)
                {
                    mcp23x1x = mcp23xxxWrite as Mcp23x1x;
                }

                if (mcp23x1x == null)
                {
                    Console.WriteLine("LitAllEmptySpots: Unable to cast Mcp23017 Write GPIO.");   
                }
                else
                {
                    mcp23x1x.WriteByte(Register.GPIO, (byte) (lastDataPortA ^ 255) , Port.PortA);
                    mcp23x1x.WriteByte(Register.GPIO, (byte) (lastDataPortB ^255), Port.PortB);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error when LitAllEmptySpots: {ex.Message}");
            }
            finally
            {
                _ledsPlaying = false;
            }
        }

        public static async Task LitAllUsedSpots(Mcp23xxx mcp23xxxWrite, byte lastDataPortA, byte lastDataPortB)
        {
            try
            {
                while(_ledsPlaying)
                {
                    // let the previous light show end.
                    await Task.Delay(5);
                }

                _ledsPlaying = true;

                Mcp23x1x mcp23x1x = null;
                
                if (mcp23xxxWrite != null)
                {
                    mcp23x1x = mcp23xxxWrite as Mcp23x1x;
                }

                if (mcp23x1x == null)
                {
                    Console.WriteLine("LitAllUsedSpots: Unable to cast Mcp23017 Write GPIO.");   
                }
                else
                {
                    mcp23x1x.WriteByte(Register.GPIO, lastDataPortA , Port.PortA);
                    mcp23x1x.WriteByte(Register.GPIO, lastDataPortB, Port.PortB);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error when LitAllUsedSpots: {ex.Message}");
            }
            finally
            {
                _ledsPlaying = false;
            }
        }


        public static async Task SwitchOffAllSpots(Mcp23xxx mcp23xxxWrite, byte lastDataPortA, byte lastDataPortB)
        {
            try
            {
                while(_ledsPlaying)
                {
                    // let the previous light show end.
                    await Task.Delay(5);
                }

                _ledsPlaying = true;

                Mcp23x1x mcp23x1x = null;
                
                if (mcp23xxxWrite != null)
                {
                    mcp23x1x = mcp23xxxWrite as Mcp23x1x;
                }

                if (mcp23x1x == null)
                {
                    Console.WriteLine("SwtichoffAllSpots: Unable to cast Mcp23017 Write GPIO.");   
                }
                else
                {
                    mcp23x1x.WriteByte(Register.GPIO, 0 , Port.PortA);
                    mcp23x1x.WriteByte(Register.GPIO, 0, Port.PortB);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error when SwtichoffAllSpots: {ex.Message}");
            }
            finally
            {
                _ledsPlaying = false;
            }
        }

        public static async Task<bool> DirectFirstEmptySpot(Mcp23xxx mcp23xxxWrite, int firstEmptySlot)
        {
            if (firstEmptySlot == 0)
            {
                Console.WriteLine("Skip blink.");
                return true;
            }

            try
            {
                while(_ledsPlaying)
                {
                    // let the previous light show end.
                    await Task.Delay(5);
                }

                _ledsPlaying = true;

                Mcp23x1x mcp23x1x = null;
                
                if (mcp23xxxWrite != null)
                {
                    mcp23x1x = mcp23xxxWrite as Mcp23x1x;
                }

                if (mcp23x1x == null)
                {
                    Console.WriteLine("DirectFirstEmptySpot: Unable to cast Mcp23017 Write GPIO.");   

                    return false;
                }
                else
                {
                    mcp23x1x.WriteByte(Register.GPIO, 0 , Port.PortA);
                    mcp23x1x.WriteByte(Register.GPIO, 0, Port.PortB);

                    var port = firstEmptySlot <= 8 ? Port.PortA : Port.PortB;

                    byte bPos = firstEmptySlot <= 8 
                                        ? (byte) Math.Pow(2, firstEmptySlot -1)
                                        : (byte) Math.Pow(2, firstEmptySlot - 9);
                    
                    for (var i = 0; i<25 ; i++)
                    {
                        // blink led on i % 2 on else off
                        var j = (i % 2) == 0 ? bPos : 0;

                        Console.Write($"Lit {j}. ");

                        mcp23x1x.WriteByte(Register.GPIO, (byte) j , port);

                        await Task.Delay(100);
                    }

                    Console.WriteLine();
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                _ledsPlaying = false;
            }

            return true;
        }


        public static async Task<bool> DirectCircus(Mcp23xxx mcp23xxxWrite)
        {
            try
            {
                while(_ledsPlaying)
                {
                    // let the previous light show end.
                    await Task.Delay(5);
                }

                _ledsPlaying = true;

                Mcp23x1x mcp23x1x = null;
                
                if (mcp23xxxWrite != null)
                {
                    mcp23x1x = mcp23xxxWrite as Mcp23x1x;
                }

                if (mcp23x1x == null)
                {
                    Console.WriteLine("DirectCircus: Unable to cast Mcp23017 Write GPIO.");   

                    return false;
                }
                else
                {
                    Console.WriteLine("Ra da da da da da da da Circus");
                    Console.WriteLine("Da da da da da da da da");
                    Console.WriteLine("Afro Circus, Afro Circus, Afro");
                    Console.WriteLine("Polka dot, polka dot, polka dot, Afro!");

                    for(var i = 0; i< 256; i++)
                    {
                        mcp23x1x.WriteByte(Register.GPIO, (byte) i , Port.PortA);
                        mcp23x1x.WriteByte(Register.GPIO, (byte) i, Port.PortB);

                        await Task.Delay(20);
                    }  
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                _ledsPlaying = false;
            }

            return true;
        }

        public static async Task<bool> DirectLedTest(Mcp23xxx mcp23xxxWrite, int ledPosition)
        {
            try
            {
                while(_ledsPlaying)
                {
                    // let the previous light show end.
                    await Task.Delay(5);
                }

                _ledsPlaying = true;

                Mcp23x1x mcp23x1x = null;
                
                if (mcp23xxxWrite != null)
                {
                    mcp23x1x = mcp23xxxWrite as Mcp23x1x;
                }

                if (mcp23x1x == null)
                {
                    Console.WriteLine("DirectLedTest: Unable to cast Mcp23017 Write GPIO.");   

                    return false;
                }
                else
                {
                    mcp23x1x.WriteByte(Register.GPIO, 0 , Port.PortA);
                    mcp23x1x.WriteByte(Register.GPIO, 0, Port.PortB);

                    var port = ledPosition <= 8 ? Port.PortA : Port.PortB;

                    byte bPos = ledPosition <= 8 
                                        ? (byte) Math.Pow(2, ledPosition -1)
                                        : (byte) Math.Pow(2, ledPosition - 9);
                    
                    for (var i = 0; i<25 ; i++)
                    {
                        if (ledPosition == 0)
                        {
                            Console.Write("Skip blink. ");
                            continue;
                        }

                        // blink led on i % 2 on else off
                        var j = (i % 2) == 0 ? bPos : 0;

                        mcp23x1x.WriteByte(Register.GPIO, (byte) j , port);

                        await Task.Delay(100);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                _ledsPlaying = false;
            }

            return true;
        }

        public static async Task PlayUpScene(Mcp23xxx mcp23xxxWrite, int UpDownInterval)
        {
            try
            {
                while(_ledsPlaying)
                {
                    // let the previous light show end.
                    await Task.Delay(5);
                }

                _ledsPlaying = true;

                Mcp23x1x mcp23x1x = null;
                
                if (mcp23xxxWrite != null)
                {
                    mcp23x1x = mcp23xxxWrite as Mcp23x1x;
                }

                if (mcp23x1x == null)
                {
                    Console.WriteLine("PlayUpScene: Unable to cast Mcp23017 Write GPIO.");   
                }
                else
                {
                    // Use UpDownInterval to predict how long the scen must play 

                    var sleepInterval = 100;

                    var j = UpDownInterval / sleepInterval;

                    byte a = 0b_0000_0001;

                    for(var i = 0; i< j; i++)
                    {
                        var shifter = (i % 8);

                        int b = a << shifter;

                        mcp23x1x.WriteByte(Register.GPIO, (byte) b , Port.PortA);
                        mcp23x1x.WriteByte(Register.GPIO, (byte) b, Port.PortB);

                        await Task.Delay(sleepInterval);
                    }

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error when PlayUpScene: {ex.Message}");
            }
            finally
            {
                _ledsPlaying = false;
            }
        }


        public static async Task PlayDownScene(Mcp23xxx mcp23xxxWrite, int UpDownInterval)
        {

            try
            {
                while(_ledsPlaying)
                {
                    // let the previous light show end.
                    await Task.Delay(5);
                }

                _ledsPlaying = true;

                Mcp23x1x mcp23x1x = null;
                
                if (mcp23xxxWrite != null)
                {
                    mcp23x1x = mcp23xxxWrite as Mcp23x1x;
                }

                if (mcp23x1x == null)
                {
                    Console.WriteLine("PlayDownScene: Unable to cast Mcp23017 Write GPIO.");   
                }
                else
                {
                    // Use UpDownInterval to predict how long the scen must play 

                    var sleepInterval = 100;

                    var j = UpDownInterval / sleepInterval;

                    byte a = 0b_1000_0000;

                    for(var i = 0; i< j; i++)
                    {
                        var shifter = ((i + 8) % 8);

                        int b = a >> shifter;

                        mcp23x1x.WriteByte(Register.GPIO, (byte) b , Port.PortA);
                        mcp23x1x.WriteByte(Register.GPIO, (byte) b, Port.PortB);

                        await Task.Delay(sleepInterval);
                    }

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error when PlayUpScene: {ex.Message}");
            }
            finally
            {
                _ledsPlaying = false;
            }
        }
    }
}
