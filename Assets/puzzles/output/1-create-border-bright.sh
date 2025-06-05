#!/bin/bash

for x; do
	magick $x -fuzz 30% -transparent white tmp1.png
	magick tmp1.png -fuzz 40% -fill white -opaque black tmp2.png
	magick tmp2.png -background "rgb(0,0,0)" -alpha background -alpha off tmp3.png   # replace transparent pixels with 20,20,20
	magick tmp3.png -compose copy -bordercolor white -border 17 1-border-bright/$( basename $x )  # add border
done
rm tmp1.png tmp2.png tmp3.png
