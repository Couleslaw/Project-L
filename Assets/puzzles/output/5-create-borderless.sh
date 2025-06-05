#!/bin/bash

for x; do
	magick $x -fuzz 30% -transparent white tmp.png
	magick tmp.png -fuzz 40% -fill white -opaque black 5-borderless/$( basename $x )
done
rm tmp.png
