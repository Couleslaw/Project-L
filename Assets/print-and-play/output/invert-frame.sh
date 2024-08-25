#!/bin/bash

for x; do
	magick convert -bordercolor black -border 17 $x tmp1.png
	magick convert -fuzz 30% -transparent white tmp1.png tmp2.png
	magick convert -fuzz 17% -fill white -opaque black tmp2.png frame/$x
done
rm tmp1.png tmp2.png
