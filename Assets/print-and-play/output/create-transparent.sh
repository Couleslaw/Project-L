#!/bin/bash

for x; do
	magick $x -fuzz 30% -transparent white tmp.png
	magick tmp.png -fuzz 17% -fill white -opaque black inverted/$( basename $x )
done
rm tmp.png
