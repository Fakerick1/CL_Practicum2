﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SudokuSolver
{
    public class Sudoku
    {
        public const int SudokuSize = 9;
        public const int SudokuBlockSize = 3;

        private SudokuNode[,] nodeArray;
        private Stack<DomainChangeList> changes;

        private int amountOfSteps;
        private int correctNodes;

        public Sudoku(string input)
        {
            this.nodeArray = new SudokuNode[SudokuSize, SudokuSize];
            this.amountOfSteps = 0;
            this.changes = new Stack<DomainChangeList>();

            // Generate and display starting state
            ConvertInputToNodeArray(input);

            // Ensure starting nodes are node-consistent
            EnsureNodeConsistency();
        }

        public void Solve()
        {
            // Use chronological backtracking by generating the nodes dynamically in depth-first order
            // Loop through rows
            for (int i = 0; i < SudokuSize; i++)
            {
                // Loop through columns
                for (int j = 0; j < SudokuSize; j++)
                {
                    if (!nodeArray[i, j].IsFixed())
                    {
                        if (nodeArray[i, j].SetFirstValue())
                        {
                            EnsureLocalNodeConsistency(i, j);
                            correctNodes++;
                        } else
                        {
                            (int row, int column) source = UndoLastStep();
                            if (source.column == 0)
                            {
                                i = source.row - 1;
                                j = 8;
                            } else
                            {
                                i = source.row;
                                j = source.column - 1;
                            }
                            correctNodes--;
                        }
                    }
                    if (SudokuSolver.PrettyPrint) PrintSudoku();

                    amountOfSteps++;
                }
            }
            if (correctNodes != SudokuSize * SudokuSize)
            {
                Console.WriteLine("Sudoku was not finished correctly");
            }
        }

        public int GetAmountOfSteps()
        {
            return amountOfSteps;
        }

        private void EnsureLocalNodeConsistency(int i, int j)
        { 
            DomainChangeList domainChanges = new DomainChangeList((i,j));
            
            // Loop through rows and columns
            for (int k = 0; k < SudokuSize; k++)
            {
                if (!this.nodeArray[i, k].IsFixed())
                {
                    RemoveFromDomain(i, k, this.nodeArray[i, j].Value(), domainChanges);                    
                }
                if (!this.nodeArray[k, j].IsFixed())
                {
                    RemoveFromDomain(k, j, this.nodeArray[i, j].Value(), domainChanges);
                }
            }

            // Loop through 3*3 block
            int blockRowIndex = (i / SudokuBlockSize) * SudokuBlockSize;
            int blockColumnIndex = (j / SudokuBlockSize) * SudokuBlockSize;

            for (int l = blockRowIndex; l < blockRowIndex + SudokuBlockSize; l++)
            {
                for (int m = blockColumnIndex; m < blockColumnIndex + SudokuBlockSize; m++)
                {
                    if (!this.nodeArray[l, m].IsFixed())
                    {
                        RemoveFromDomain(l, m, this.nodeArray[i, j].Value(), domainChanges);
                    }
                }
            }
            changes.Push(domainChanges);
        }

        private void RemoveFromDomain(int i, int j, int value, DomainChangeList domainChanges)
        {
            if (this.nodeArray[i, j].RemoveValue(value))
            {
                domainChanges.AddDomainChange(new DomainChange(i, j, value));
            }
        }

        private (int i, int j) UndoLastStep()
        {
            DomainChangeList domainChanges = changes.Pop();

            // Undo changes to domains that occurred in this step
            foreach (DomainChange domainChange in domainChanges.DomainChanges())
            {
                this.nodeArray[domainChange.Row(), domainChange.Column()].AddValue(domainChange.Value());
            }

            // Remove the invalid value from the domain of the node and add it to the domainchanges list of the previous change
            int i = domainChanges.Source().i;
            int j = domainChanges.Source().j;
            int value = this.nodeArray[i, j].Value();

            if (changes.Count != 0)
            {
                DomainChangeList lastChanges = changes.Pop();
                if (this.nodeArray[i, j].RemoveValue(value))
                {
                    lastChanges.AddDomainChange(new DomainChange(i, j, value));
                }
                changes.Push(lastChanges);
            } else
            {
                // When this is the first node, the value cannot and does not need to be added to the domainchanges list of the previous change
                this.nodeArray[i, j].RemoveValue(value);
            }

            return domainChanges.Source();
        }
        
        private void EnsureNodeConsistency()
        {
            // Loop through rows
            for (int i = 0; i < SudokuSize; i++)
            {
                // Loop through columns
                for (int j = 0; j < SudokuSize; j++)
                {
                    // If the node is Fixed, propagate the value to those sudokuNodes in nodeArray that are not fixed and are in the same row, column, or block
                    if (this.nodeArray[i, j].IsFixed())
                    {
                        EnsureLocalNodeConsistency(i, j);
                    }
                }
            }
            // Clear stack for fresh start of chronological backtracking
            changes.Clear();
        }


        private void ConvertInputToNodeArray(string input)
        {
            // Trim input since input is always given with a white space, will also work with input where this is not the case
            int[] inputArray = input.Trim().Split(' ').Select(int.Parse).ToArray();

            // Loop through rows
            for (int i = 0; i < SudokuSize; i++)
            {
                // Loop through columns
                for (int j = 0; j < SudokuSize; j++)
                {
                    // Fill nodeArray with SudokuNodes, isFixed = true when a number other than 0 is the input
                    bool isFixed = inputArray[(i * SudokuSize) + j] != 0;
                    this.nodeArray[i, j] = new SudokuNode(inputArray[(i * SudokuSize) + j], isFixed);
                    if (isFixed) correctNodes++;
                }
            }
        }
        public void PrintSudoku()
        {
            // Loop through rows
            for (int i = 0; i < SudokuSize; i++)
            {
                // Loop through columns
                for (int j = 0; j < SudokuSize; j++)
                {
                    Console.ForegroundColor = (this.nodeArray[i, j].IsFixed()) ? ConsoleColor.Blue : ConsoleColor.Black;

                    Console.BackgroundColor = (this.nodeArray[i, j].Value() == 0) ? ConsoleColor.Red : ConsoleColor.Green;

                    if (this.nodeArray[i, j] != null)
                    {
                        Console.Write(this.nodeArray[i, j].Value());
                    }
                    else
                    {
                        Console.Write(0);
                    }
                    
                    Console.Write(" ");
                    if (j % 3 == 2 && j != SudokuSize - 1)
                    {
                        Console.BackgroundColor = ConsoleColor.Black;
                        Console.Write(" ");
                    }

                    Console.ResetColor();
                }
                Console.WriteLine();
                if (i % 3 == 2 && i != SudokuSize - 1)
                {
                    Console.WriteLine();
                }
                if (i == SudokuSize - 1 & SudokuSolver.PrettyPrint)
                {
                    Console.WriteLine("--------------------");
                }
            }
        }
    }
}