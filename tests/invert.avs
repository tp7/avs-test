# name: Invert Test
# test case: width=1280, height=720, csp="RGB32"
# test case: width=126, height=16, csp="YV24"
# test case: {{ name: invert; img-name: invert_rgb; vars: width=1920, height=1080, csp="RGB32" }}
width=1920
height=1080
csp="YV24"
colorbars(width, height, csp)
invert()

