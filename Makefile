include Makefile.helpers
modname = AntiAlchemyAbuse
dependencies =

assemble:
	rm -f -r public
	mkdir -p public/$(pluginpath)/$(modname)
	cp -u bin/$(modname).dll public/$(pluginpath)/$(modname)/

forceinstall:
	make assemble
	rm -r -f $(gamepath)/$(pluginpath)/$(modname)
	cp -u -r public/* $(gamepath)

play:
	(make install && cd .. && make play)
