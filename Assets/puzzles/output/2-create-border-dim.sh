#!/bin/bash

for x; do
	magick $x -fuzz 30% -transparent white tmp1.png
	magick tmp1.png -fuzz 40% -fill white -opaque black tmp2.png
	magick tmp2.png -background "rgb(0,0,0)" -alpha background -alpha off tmp3.png   # replace transparent pixels
	magick tmp3.png -compose copy -bordercolor "rgb(40,40,40)" -border 10 2-border-dim/$( basename $x )  # add border
done
rm tmp1.png tmp2.png tmp3.png
