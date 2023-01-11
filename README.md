# vergiFinance & vergiCommon
Finance utilities for personal use

## vergiFinance
Ease and automate finance stuff that would otherwise require exceling/calculator
* ReadTransactions: 
  * Read ledger data from various sources (currently only Kraken supported)
  * Create sales event log that is easy to manipulate programmatically
  * Use sales log to generate profits/losses tax report (for finnish Vero)
  * Use sales log to generate staking reward tax report (for finnish Vero). The calculations fetch ticker market data for all staking reward days and sums these up.
  * Support extended to other equity types and events if deemed necessary.
* Functions - simple functions for small tedious tasks
  * CalculateDueDate
  * CalculateWorkDaysForMonth
  * GenerateSalesEstimateReport


### Roadmap
* Mock data
* Simple config whether to use mock or real data
* More transaction parsing and definitions support


## vergiCommon
Common utilities used in personal projects. Nuget-versioned for sharing between repos. Will be separated to own repo if grows to much.
* GetPath - paths like sln/proj folder, assembly folder, mydocs for local development in windows file system
* Get
  * IInput ReadInput
  * IFile ReadFile
  * ICsvFile ReadCsvFile
  * IFile ReadSingleFileFromZip
  * IFile ReadFileFromZip
  * IReadOnlyList\<string\> ReadFileNamesFromZip
