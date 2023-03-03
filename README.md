# drifts-helper
A small helper for integration of multiple spectra regions in TR-DRIFTS spectra series

This app expects TR-DRIFTS data in multiple `*.csv` files in a directory. Files are sorted by name (descending) prior to processing.

It outputs integral intensity curves (`*.csv`) for specified spectrum regions and it also can batch-subtract one spectra from another.

Arguments:
 - `-r --region`: integration regions, example `-r 1600,1700 2200,2300`
 - `-d --diff`: spectra difference (subtraction), expects spectra indices, example `-d 0,2 24,65 50,60`,
 first number is the spectrum to be subtracted, second one is the spectrum to subtract from ("subtract #0 from #2")
 - `-f --folder`: specify a single folder to process files from.
 The folder can contain a subfolder named `spectra`, in this case the subfolder will be selected for processing instead.
 - `-p --path`: specify a path for recursive folder processing. Each folder inside specified directory will be processed.
 Again, each folder can have a `spectra` subfolder, and it will be selected if found.
 - `-o --output`: output file name. If not specified, container folder name (with operation type appended, e.g. `_int`) will be used.
 - `-m --method`: if this switch is passed, peak heights will be used instead of integration to compute intensity.
 
Note: baseline for integration or peak finding is assumed to be linear, and is computed using averaged endpoints of the interval.
There is a parameter that controls the number of points outside the interval used to compute averaged enpoint heights, but there's no CLI for it yet.
Default is 15 points on each side.
