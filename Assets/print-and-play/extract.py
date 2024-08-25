#!/usr/bin/env py

from PIL import Image
import sys

OUTPUT_DIR = 'output'

def extract_images_from_file(path: str, index: int, name) -> int: 
    # Load the PNG file
    image = Image.open(path)

    # Define the dimensions of the grid
    y_margin = 197
    x_margin = 67

    padding = 10 
    
    grid_width = 617 - x_margin + 1
    grid_height = 826 - y_margin + 1


    # Iterate through the grid and extract each image
    j = index
    for y in range(y_margin, image.height - grid_height, grid_height):
        for x in range(x_margin, image.width - grid_width, grid_width):
            j+=1
            # Define the region to crop
            box = (x+padding, y+padding, x + grid_width-padding, y + grid_height-padding)
            # Crop the image
            cropped_image = image.crop(box)
            # Save the cropped image
            cropped_image.save(f'{OUTPUT_DIR}/{name}-{str(j).zfill(2)}.png')
    return j

def main(args):
    j = 0
    name = args[0]
    for path in args[1:]:
        j = extract_images_from_file(path, j, name)

if __name__ == '__main__':
    main(sys.argv[1:])
