# name: Invert Test
# test case: width=1280, height=720, csp="RGB32"
# test case: {{ name: invert yv24 performance test; vars: width=1920, height=1080, csp="YV24"; kind: perf; frames: 1000; skip: 100 }}
width=1920
height=1080
csp="YV24"
colorbars(width, height, csp)
invert()
