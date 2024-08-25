#!/bin/bash

for x; do
	magick convert -fuzz 30% -transparent white $x tmp.png
	magick convert -fuzz 17% -fill white -opaque black tmp.png inverted/$x
done
rm tmp.png
