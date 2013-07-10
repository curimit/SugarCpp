LEX=flex
YACC=bison -d
CC=g++ -O2
FLAGS=-c -std=gnu++11
SC=SugarCpp

AstSugar=AstNode TargetCpp TargetCppHeader TargetCppImplementation Compiler
AstCpp=cAstNode StringTemplate cRender

AstSugarSc=$(foreach file, $(AstSugar), src/AstSugar/$(file).sc)
AstSugarCpp=$(foreach file, $(AstSugar), src/AstSugar/$(file).h src/AstSugar/$(file).cpp)

AstCppSc=$(foreach file, $(AstCpp), src/AstCpp/$(file).sc)
AstCppCpp=$(foreach file, $(AstCpp), src/AstCpp/$(file).h src/AstCpp/$(file).cpp)

SugarCpp: bin/lex.yy.o bin/yacc.tab.o bin/main.o bin/common.o $(AstSugar) $(AstCpp)
	@echo 'link...'
	@mkdir -p ./bin
	$(CC) bin/*.o -o bin/SugarCpp

run: SugarCpp
	@clear
	@echo "[result]"
	@./bin/SugarCpp

yacc:
	$(LEX) src/yacc/lex.l
	$(YACC) src/yacc/yacc.y
	@rm *.c *.h

# object
$(AstCpp): $(AstCppCpp) common
	@mkdir -p ./bin
	$(CC) $(FLAGS) src/AstCpp/$@.cpp -o bin/$@.o

$(AstSugar): $(AstSugarCpp) common
	@mkdir -p ./bin
	$(CC) $(FLAGS) src/AstSugar/$@.cpp -o bin/$@.o

bin/main.o: src/main.h src/main.cpp common
	@mkdir -p ./bin
	$(CC) $(FLAGS) src/main.cpp -o bin/main.o

bin/common.o: common
	@mkdir -p ./bin
	$(CC) $(FLAGS) src/common.cpp -o bin/common.o

bin/lex.yy.o: src/yacc/lex.yy.c src/yacc/yacc.tab.h common
	@mkdir -p ./bin
	$(CC) $(FLAGS) src/yacc/lex.yy.c -o bin/lex.yy.o

bin/yacc.tab.o: src/yacc/yacc.tab.c src/main.h common
	@mkdir -p ./bin
	$(CC) $(FLAGS) src/yacc/yacc.tab.c -o bin/yacc.tab.o

common: src/main.h src/main.cpp src/common.h src/common.cpp $(AstCppCpp) $(AstSugarCpp)

# SugarCpp
src/main.h src/main.cpp: src/main.sc
	$(SC) -nocode src/main.sc

src/common.h src/common.cpp: src/common.sc
	$(SC) -nocode src/common.sc

$(AstCppCpp): $(AstCppSc)
	$(SC) -nocode $(AstCppSc)

$(AstSugarCpp): $(AstSugarSc)
	$(SC) -nocode $(AstSugarSc)

# yacc
src/yacc/yacc.tab.c src/yacc/yacc.tab.h: src/yacc/yacc.y
	$(YACC) src/yacc/yacc.y
	mv yacc.tab.* src/yacc

src/yacc/lex.yy.c: src/yacc/lex.l
	$(LEX) src/yacc/lex.l
	mv lex.yy.c src/yacc

clean:
	@rm -f bin/*.o
	@rm -f src/*.h src/*.cpp src/yacc/*.c src/yacc/*.h *.cpp *.h
	@rm -f $(AstSugarCpp) $(AstCppCpp)
