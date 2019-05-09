using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
namespace Assembler
{
    class Program
    {
        public static Dictionary<string, int> Rcodes = new Dictionary<string, int>();
        public static Dictionary<string, int> IJcodes = new Dictionary<string, int>();
        public static  Dictionary<string, int> instructionsNumbers = new Dictionary<string, int>();
        public static Dictionary<string, int> dataLabels = new Dictionary<string, int>();
        public static Dictionary<string, int> codeLabels = new Dictionary<string, int>();
        public static List<string> dataTrasnlation = new List<string>();
        public static List<string> codeTranslation = new List<string>();


        public static bool Compare(string file1, string file2)
        {
            // assume that the two files are identical
            bool flag = true;
            FileStream stream1 = new FileStream(file1, FileMode.Open);
            StreamReader SR1 = new StreamReader(stream1);
            FileStream stream2 = new FileStream(file2, FileMode.Open);
            StreamReader SR2 = new StreamReader(stream2);
            int i = 0;
            while (SR1.Peek() != -1)
            {
                string s1 = SR1.ReadLine();
                string s2 = SR2.ReadLine();
                ++i;
                if (s1 != s2)
                {
                    // if the two lines are not identical print them
                    Console.WriteLine(i + " " + s1 + " " + s2);
                    // the two files are not identical
                    flag = false;
                }
            }
            return flag;
        }

        public static List<string> CleanLine(string instruction)
        {
            List<string> splittedLine = instruction.Split(new Char[] { ',', ' ','(' }).OfType<string>().ToList();
            for (int i = 0; i < splittedLine.Count; ++i)
            {
                if (splittedLine[i] == "")
                {
                    splittedLine.RemoveAt(i);
                    --i;
                }
            }
            int hashidx = -1;
            for (int i = 0; i < splittedLine.Count; ++i)
            {
                if (splittedLine[i][0] == '#')
                {
                    hashidx = i;
                    break;
                }
            }
            if (hashidx != -1)
            {
                int count = splittedLine.Count - hashidx;
                for (int i = 0; i < count; ++i)
                    splittedLine.RemoveAt(splittedLine.Count - 1);
            }
            return splittedLine;
        }

        public static string TranslateInstruction(List<string> splittedLine, int label, int programCounter)
        {
            string machineCode = "";
            if (Rcodes.ContainsKey(splittedLine[0+label]))
            {
                int funct = 0, op = 0, shamt = 0, rs = 0, rt = 0, rd = 0;
                funct = Rcodes[splittedLine[0+label]];
                rd = instructionsNumbers[splittedLine[1+label]];
                rs = instructionsNumbers[splittedLine[2+label]];
                rt = instructionsNumbers[splittedLine[3+label]];

                machineCode += Convert.ToString(op, 2).PadLeft(6, '0');
                machineCode += Convert.ToString(rs, 2).PadLeft(5, '0');
                machineCode += Convert.ToString(rt, 2).PadLeft(5, '0');
                machineCode += Convert.ToString(rd, 2).PadLeft(5, '0');
                machineCode += Convert.ToString(shamt, 2).PadLeft(5, '0');
                machineCode += Convert.ToString(funct, 2).PadLeft(6, '0');

            }
            else
            {
                if (splittedLine[0+label] == "sw" || splittedLine[0+label] == "lw")
                {
                    int op = IJcodes[splittedLine[0+label]];
                    string lastReg = splittedLine[3+label];
                    lastReg = lastReg.Remove(lastReg.Length - 1);
                    int rs = instructionsNumbers[lastReg];
                    int rt = instructionsNumbers[splittedLine[1+label]];
                    int address;

                    if (dataLabels.ContainsKey(splittedLine[2+label]))
                    {
                        address = dataLabels[splittedLine[2+label]];
                    }
                    else
                    {
                        address = int.Parse(splittedLine[2+label]);
                    }
                    machineCode += Convert.ToString(op, 2).PadLeft(6, '0');
                    machineCode += Convert.ToString(rs, 2).PadLeft(5, '0');
                    machineCode += Convert.ToString(rt, 2).PadLeft(5, '0');
                    machineCode += Convert.ToString(address, 2).PadLeft(16, '0');
                }
                else if (splittedLine[0+label] == "bne" || splittedLine[0+label] == "beq")
                {
                    int op = IJcodes[splittedLine[0+label]];
                    int rs = instructionsNumbers[splittedLine[1+label]];
                    int rt = instructionsNumbers[splittedLine[2+label]];
                    int distance = (codeLabels[splittedLine[3+label]] / 4) - programCounter;
                    machineCode += Convert.ToString(op, 2).PadLeft(6, '0');
                    machineCode += Convert.ToString(rs, 2).PadLeft(5, '0');
                    machineCode += Convert.ToString(rt, 2).PadLeft(5, '0');

                    if (distance > 0)
                    {
                        machineCode += Convert.ToString(distance, 2).PadLeft(16, '0');
                    }
                    else
                    {
                        machineCode += Convert.ToString(distance, 2).PadLeft(16, '1');
                        machineCode = machineCode.Remove(16, 16);
                    }

                }
                else
                {
                    int op = IJcodes[splittedLine[0+label]];
                    int address = codeLabels[splittedLine[1+label]] / 4;
                    machineCode += Convert.ToString(op, 2).PadLeft(6, '0');
                    machineCode += Convert.ToString(address, 2).PadLeft(26, '0');


                }


            }
            return machineCode;


        }


        public static void FillData()
        {
            IJcodes["addi"] = 8; IJcodes["lw"] = 35; IJcodes["sw"] = 43;
            IJcodes["beq"] = 4; IJcodes["bne"] = 5; IJcodes["j"] = 2;
            Rcodes["add"] = 32; Rcodes["sub"] = 34; Rcodes["and"] = 36;
            Rcodes["nor"] = 39; Rcodes["or"] = 37; Rcodes["slt"] = 42;

            instructionsNumbers["$zero"] = 0; instructionsNumbers["$at"] = 1;
            for (int i = 0; i < 2; i++) instructionsNumbers["$v" + i.ToString()] = i + 2;
            for (int i = 0; i < 4; i++) instructionsNumbers["$a" + i.ToString()] = i + 4;
            for (int i = 0; i < 8; i++) instructionsNumbers["$t" + i.ToString()] = i + 8;
            for (int i = 0; i < 8; i++) instructionsNumbers["$s" + i.ToString()] = i + 16;
            for (int i = 0; i < 2; i++) instructionsNumbers["$t" + (i + 8).ToString()] = i + 24;
            for (int i = 0; i < 2; i++) instructionsNumbers["$k" + i.ToString()] = i + 26;
            instructionsNumbers["$gp"] = 28; instructionsNumbers["$sp"] = 29;
            instructionsNumbers["$fp"] = 30; instructionsNumbers["$ra"] = 31;


        }

        static void Main(string[] args)
        {
            FillData();
            FileStream stream = new FileStream("input.txt", FileMode.Open);
            StreamReader reader = new StreamReader(stream);

            FileStream dataStream = new FileStream("dataoutput.txt", FileMode.Create);
            StreamWriter dataWriter = new StreamWriter(dataStream);

            FileStream codeStream = new FileStream("codeoutput.txt", FileMode.Create);
            StreamWriter codeWriter = new StreamWriter(codeStream);



            int dataCounter = -1;
            int programCounter = -1;

            bool dataSegment = true;
            while (reader.Peek() != -1)
            {
                string instruction = reader.ReadLine().ToLower();
                if (instruction == "" || instruction[0] == '#') continue;

                List<string> splittedLine = CleanLine(instruction);
             
                if (instruction == ".text")
                {
                    dataSegment = false;
                }
                if(dataSegment)
                {
                    if (dataCounter == -1)
                    {
                        dataCounter = 0;
                        continue;
                    }

                    string name = splittedLine[0];

                    if(name[name.Length-1] == ':')
                    {
                        name = name.Remove(name.Length - 1);
                    }
                    else if(splittedLine[1] == ":")
                    {
                        splittedLine.RemoveAt(1);
                    }
                    else
                    {
                        splittedLine[1] = splittedLine[1].Substring(1);
                    }
                    dataLabels[name] = dataTrasnlation.Count*4;
                    string dataType = splittedLine[1];
                    if (splittedLine.Count == 3)
                    {
                        if(dataType == ".word" )
                        {
                          
                            int number = int.Parse(splittedLine[2]);
                            dataTrasnlation.Add(Convert.ToString(number, 2).PadLeft(32, '0'));                            
                        }
                        else
                        {
                            int size = int.Parse(splittedLine[2]);
                            for(int i = 0; i < size; ++i)
                            {
                                string unitialized = new String('X', 32);
                                dataTrasnlation.Add(unitialized);
                            }
                        }
                    }
                    else
                    {
                        for (int i = 2; i < splittedLine.Count; ++i)
                        {
                            int number = int.Parse(splittedLine[i]);
                            dataTrasnlation.Add(Convert.ToString(number, 2).PadLeft(32, '0'));
                        }
                    }
                }
                else
                {
                    ++programCounter;
                    if (programCounter == 0)
                    {
                        continue;
                    }
                    if(splittedLine[0][splittedLine[0].Length-1] == ':')
                    {
                        string name = splittedLine[0];
                        name = name.Remove(name.Length - 1);
                        codeLabels[name] = (programCounter-1) * 4;
                    }
                    else if (splittedLine[1] == ":")
                    {
                        splittedLine.RemoveAt(1);
                        string name = splittedLine[0];
                        codeLabels[name] = (programCounter - 1) * 4;


                    }
                    else if (splittedLine[1][0] == ':')
                    {
                        splittedLine[1].Remove(0);
                        string name = splittedLine[0];
                        codeLabels[name] = (programCounter - 1) * 4;

                    }

                }
            }

            for (int i = 0; i < dataTrasnlation.Count; ++i)
                dataWriter.WriteLine("MEMORY("+i.ToString()+ ") <= \"" + dataTrasnlation[i]+ "\"" + " ;");

            dataWriter.Close();
            reader.Close();

            stream = new FileStream("input.txt", FileMode.Open);
            reader = new StreamReader(stream);
 
            programCounter = -1;
            dataSegment = true;

            
            while (reader.Peek() != -1)
            {
                string instruction = reader.ReadLine().ToLower();
                if (instruction == "" || instruction[0] == '#') continue;

                List<string> splittedLine = CleanLine(instruction);

                if (instruction == ".text")
                {
                    dataSegment = false;
                }

                if (!dataSegment)
                {
                   
                    ++programCounter;
                    if (programCounter == 0)
                    {
                        continue;
                    }
                    string machineCode = "";
                    if (splittedLine[0][splittedLine[0].Length - 1] == ':')
                    {
                        machineCode = TranslateInstruction(splittedLine, 1, programCounter) ;
                    }
                    else if( splittedLine[1]==":")
                    {
                        splittedLine.RemoveAt(1);
   
                        splittedLine[0] += ":";
                        machineCode = TranslateInstruction(splittedLine, 1, programCounter);

                    }
                    else if(splittedLine[1][0] ==':')
                    {
                        splittedLine[1] = splittedLine[1].Substring(1);
                        splittedLine[0] += ":";
                        machineCode = TranslateInstruction(splittedLine, 1, programCounter);

                    }
                    else
                    {
                        machineCode = TranslateInstruction(splittedLine, 0, programCounter);
                    }
                    codeTranslation.Add(machineCode);
                }
            }

            for (int i = 0; i < codeTranslation.Count; ++i)
                codeWriter.WriteLine("MEMORY(" + i.ToString() + ") := \"" + codeTranslation[i] + "\"" + " ;");
            codeWriter.Close();

            bool identical = Compare("dout.txt", "dataoutput.txt");
             identical = Compare("cout.txt", "codeoutput.txt");


        }
    }
}
