using UnityEngine;	
using System;
using Assets.My_Assets.dinoScripts;

public class FuzzyLogic : MonoBehaviour{
	double[] run = new double[] {1,1,1,.7,.4,.1,0,0,0,0,0};
	double[] reproduce = new double[] {0,0,0,.4,.5,.5,.5,.4,0,0,0};//{0,0,0,.4,.7,1,.7,.4,0,0,0};
	double[] eat = new double[] {0,0,0,0,0,.1,.4,.7,1,1,1};

	public Dinosaur.Priorities calPriority(PathNode actualNode,double maxStamina,double maxLifetime,double stamina,double lifetime,double hp,double maxHp){

		/**
		 *REEGLAS: 
		 * 
		 *  1.- si se tiene baja stamina entonces comer.
		 *  2.- si se tiene alta stamina y una etapa adulta entonces se reproduce.
		 *  3.- si hay muchos rivales o medios rivales y se es viejo o pocos companeros entonces corre
		 *  3.1.- si hay muchos rivales y no se es viejo entonces corre.
		 */

		double regla1 = Math.Max(staminaBaja(stamina, maxStamina),hpBajo(hp,maxHp)); //Math.Min (1,1-comer(1)+ staminaBaja(stamina,maxStamina));
		double regla2 =  Math.Min(staminaAlta(stamina,maxStamina),adultez(lifetime,maxLifetime));
		//double regla3 = Math.Min (1,/*1-correr(1)+*/Math.Max(rivalesAlto(actualNode.getPredators()),Math.Min(rivalesMedio(actualNode.getPredators()),Math.Max(vejez(lifetime,maxLifetime),companerosBajo(actualNode.getPrays())))));
		double regla3 = Math.Min(rivalesMedio(actualNode.getPredators()),1-vejez(lifetime,maxLifetime));
		//Debug.Log (rivalesMedio(actualNode.getPredators())+"  "+(1-vejez(lifetime,maxLifetime)));
		//Fuzzifying

		double[] runFuzzy = new double[run.Length];
		Array.Copy (run,runFuzzy,run.Length);

		double[] eatFuzzy = new double[run.Length];
		Array.Copy (eat,eatFuzzy,eat.Length);

		double[] reproduceFuzzy =new double[run.Length];
		Array.Copy (reproduce,reproduceFuzzy,reproduce.Length);


		for (int i = 0; i < runFuzzy.Length; i++) {
			if(runFuzzy[i]>regla3){
				runFuzzy[i]=regla3;
			}
		
			if(eatFuzzy[i]>regla1){
				eatFuzzy[i]=regla1;
			}

			if(reproduceFuzzy[i]>regla2){
				reproduceFuzzy[i]=regla2;
			}
		}

		//Max membership defuzzification method
		int maxRun=0,maxEat=0,maxReproduce=0;

		for (int i = 0; i < runFuzzy.Length; i++) {
			if(runFuzzy[i]>runFuzzy[maxRun]){
				maxRun=i;
			}
			if(eatFuzzy[i]>eatFuzzy[maxEat]){
				maxEat=i;
			}
			if(reproduceFuzzy[i]>reproduceFuzzy[maxReproduce]){
				maxReproduce=i;
			}
		}
		//Debug.Log (runFuzzy[maxRun]+" "+eatFuzzy[maxEat]+" "+reproduceFuzzy[maxReproduce]);
		if (runFuzzy[maxRun] > 0) {
			return Dinosaur.Priorities.Run;		
		}else if (eatFuzzy[maxEat] > runFuzzy[maxRun] && eatFuzzy[maxEat] > reproduceFuzzy[maxReproduce] && eatFuzzy[maxEat] !=0){
			return Dinosaur.Priorities.Eat;
		}else if(reproduceFuzzy[maxReproduce] > runFuzzy[maxRun] && reproduceFuzzy[maxReproduce] > eatFuzzy[maxEat] && reproduceFuzzy[maxReproduce] !=0){
			return Dinosaur.Priorities.Reproduce;
		}else{
			return Dinosaur.Priorities.Obey;
		}

	}

	/*
	 * Calcula probabilidades difusas
	 * 	De entrada se requere una matriz de 3XN
	 * 	Donde cada fila representa un nodo con ( Comida, Rivales, Compañeros )
	 * 	@
	 */
	public double[] calculate( double[,] G ){
		//Evalua el grafo
		if (G.GetLength (0) != 3) {
			Debug.Log("[ERROR]:\nLa tabla de nodos solo puede tener 3 renglones.\n(Comida, Rivales y Compañeros).");
			double[] error = new double[]{};
			return error;
		}
	
		//Prepara variables
		double[] ps = new double[G.GetLength(1)];
		double comidaTotal, rivalesTotal, companerosTotal;
		//double comidaPertenencia, rivalesPertenencia, companerosPertenencia;
		double comida, rivales, companeros;
		int grafoLength = G.GetLength(1);

		int i, j;
		//Columnas del grafo
		int C_Comida    = 0;
		int C_Rivales   = 1;
		int C_Comaneros = 2;
		//Valores para defusificar
		int defusificaBajo  = 60; // 0+10+20+30   = 60
		int defusificaMedio = 150;// 40+50+60     = 150
		int defusificaAlto  = 340;// 70+80+90+100 = 340
		double[] defusificaCentroide = new double[]{0.43F,1F,0.43F,0.685F,1F,0.685F,0.5F,1F,0.5F,0F};

		//Para calcular el centroide
		double centroideDivisor = 0;
		double dividendo;

		//Cuenta los valores totales
		comidaTotal = rivalesTotal = companerosTotal = 0;

		//consola.Buffer.Text += "len("+Convert.ToString(grafoLength)+")\n";

		for ( i = 0; i < grafoLength; i++) {//Para cada nodo
			comidaTotal     = comidaTotal + G[C_Comida,i];
			rivalesTotal    = rivalesTotal+ G[C_Rivales,i];
			companerosTotal = companerosTotal + G[C_Comaneros,i];
			//consola.Buffer.Text += "comida("+Convert.ToString(G[C_Comida,i])+") rivales("+Convert.ToString(G[C_Rivales,i])+") compañeros("+Convert.ToString(G[C_Comaneros,i])+")\n";
		}
		//consola.Buffer.Text += "comida("+Convert.ToString(comidaTotal)+") rivales("+Convert.ToString(rivalesTotal)+") compañeros("+Convert.ToString(companerosTotal)+")\n";

		//Para cada nodo
		for( i = 0; i < grafoLength; i++ ){
			
			//Obtiene valor ponderado de las variables de interes
			comida     = ( (G[C_Comida,i]*100)/comidaTotal ) / 100;
			rivales    = ( (G[C_Rivales,i]*100)/rivalesTotal ) / 100;
			companeros = ( (G[C_Comaneros,i]*100)/companerosTotal ) / 100;
			
			//Aplica las reglas difusas
			// 1) Poca comida v Muchos rivales v Muchos compañeros.
			// 2) Comida regular v rivales regular v compañeros regular.
			// 3) 
			//		a) Mucha comida & ( Pocos Rivales v Pocos Compañeros v (Muchos rivales & Medios Compañeros) )
			//		b) Mucha comida & Pocos rivales & pocos compañeros
			
			// 1)
			double tmpRegla1 = Math.Max ( comidaBajo(comida), rivalesAlto( (int)( Math.Round(rivales,1) * 10 )) );
			tmpRegla1 = Math.Max ( tmpRegla1, companerosAlto((int)( Math.Round(companeros,1) * 10 )) );
			
			// 2)
			double tmpRegla2 = Math.Max ( comidaMedio(comida), rivalesMedio((int)( Math.Round(rivales,1) * 10 )) );
			tmpRegla2 = Math.Min ( tmpRegla2, companerosMedio((int)( Math.Round(companeros,1) * 10 )) );
			
			// 3)
			double tmpRegla3 = Math.Min ( rivalesAlto((int)( Math.Round(rivales,1) * 10 )), companerosMedio((int)( Math.Round(companeros,1) * 10 )) );
			tmpRegla3 = Math.Max ( tmpRegla3, companerosBajo((int)( Math.Round(companeros,1) * 10 )) );
			tmpRegla3 = Math.Max ( tmpRegla3, rivalesBajo((int)( Math.Round(rivales,1) * 10 )) );
			tmpRegla3 = Math.Min ( tmpRegla3, comidaAlto(comida) );

			
			//Obtiene el factor sobre el que se dividira en el centroide
			centroideDivisor = 0;
			for( j = 0; j < defusificaCentroide.Length; j++ ){//Para cada nodo
				switch (j) {
				case 0:
				case 1:
				case 2:
				case 3:
					centroideDivisor += Math.Min (tmpRegla1, defusificaCentroide [j]);
					/*if (debugeando) {
						consola.Buffer.Text += "+ " + Convert.ToString (Math.Min (tmpRegla1, defusificaCentroide [j])) + "\n";
					}*/
					break;
				case 4:
				case 5:
				case 6:
				case 7:
					centroideDivisor += Math.Min (tmpRegla2, defusificaCentroide [j]);
					/*if (debugeando) {
						consola.Buffer.Text += "+ " + Convert.ToString (Math.Min (tmpRegla2, defusificaCentroide [j])) + "\n";
					}*/
					break;
				case 8:
				case 9:
				case 10:
					centroideDivisor += Math.Min (tmpRegla3, defusificaCentroide [j]);
					/*if (debugeando) {
						consola.Buffer.Text += "+ " + Convert.ToString (Math.Min (tmpRegla3, defusificaCentroide [j])) + "\n";
					}*/
					break;
				}
			}

			//Guarda la probabilidad como centroide en la variable que entregara
			dividendo = (defusificaBajo * tmpRegla1) + (defusificaMedio * tmpRegla2) + (defusificaAlto * tmpRegla3);
			
			if (dividendo == 0) {
				ps [i] = 0F;
			} else {
				ps [i] = ((defusificaBajo * tmpRegla1) + (defusificaMedio * tmpRegla2) + (defusificaAlto * tmpRegla3)) / centroideDivisor;
			}
			ps [i] = Math.Round (ps [i], 2);
		}

		//Pondera los totales
		if( ps.GetLength(0) > 0 ){
			//Obtiene el total de valores difusos
			double ProbTotal = 0;
			for( i=0; i < G.GetLength(1); i++  ){
				ProbTotal += ps [i];
			}
			//Pone los valores ponderados
			for( i=0; i < G.GetLength(1); i++  ){
                if (G[C_Rivales, i] >= 0)
                {
                    ps[i] = Math.Round(((ps[i] * 100) / ProbTotal), 2);
                }
                else
                {
                    ps[i] = 0;
                }

				
			}
		}

		return ps;
	}


	//COMIDA
	protected double comidaBajo( double x ){
		int y = (int)( Math.Round(x,1) * 10 );
		double[] fit = new double[] {0,.3,.6,.8,1,1,1,1,1,1,1};
		//double[] fit = new double[] {0,0,0,0,0,0,0,.23,.5,.75,1};
		//double[] fit = new double[] {.1,.3,.5,.7,.9,1,1,1,1,1,1};
		return fit[y];
	}
	protected double comidaMedio( double x ){
		int y = (int)( Math.Round(x,1) * 10 ); 
		double[] fit = new double[] {0,0,0,.4,.7,1,.7,.4,0,0,0};
		//double[] fit = new double[] {0,0,0,0,.28,.55,.8,1,1,1,1};
		//double[] fit = new double[] {.1,.3,.5,.7,.9,1,1,1,1,1,1};
		return fit[y];
	}
	protected double comidaAlto( double x ){
		int y = (int)( Math.Round(x,1) * 10 ); 
		//double[] fit = new double[] { 0, 0, 0, 0, .6, 0.8, 1, 0.8, 0.6, 0.3, 0.1};
		double[] fit = new double[] {0,0,0,0,0,0,.2,.4,.6,.8,1};
		//double[] fit = new double[] {0,.4,.7,1,1,1,1,1,1,1,1};
		//double[] fit = new double[] {.1,.3,.5,.7,.9,1,1,1,1,1,1};
		return fit[y];
	}
	
	//RIVALES
	protected double rivalesBajo( int x ){
		if (x >= 8)x = 7;
		//double[] fit = new double[] {.6,.6,.48,.35,.22,.1,0,0,0,0,0};
		double[] fit = new double[] {1,.6,.3,0,0,0,0,0};
		return fit[x];
	}
	protected double rivalesMedio( int x ){
		if (x >= 8)x = 7; 
		//double[] fit = new double[] {1,.9,.8,.7,.6,.5,.4,.3,.2,.1,0};
		double[] fit = new double[] {0,.4,.7,.1,1,.7,.4,0};
		return fit[x];
	}
	protected double rivalesAlto( int x ){
		if (x >= 8)x = 7;
		//double[] fit = new double[] {1,1,1,1,.9,.8,.72,.63,.52,.45,.36};
		double[] fit = new double[] {0,0,0,0,0,.3,.6,1};
		return fit[x];
	}
	
	//COMPAÑEROS
	protected double companerosBajo( int x ){
		if (x >= 8)x = 7;
		//double[] fit = new double[] {.6,.88,1,.88,.7,.5,.3,0,0,0,0};
		double[] fit = new double[] {1,.6,.3,0,0,0,0,0};
		return fit[x];
	}
	protected double companerosMedio( int x ){
		if (x >= 8)x = 7;
		//double[] fit = new double[] {0,.2,.4,.6,.8,1,.8,.6,.4,.2,0};
		double[] fit = new double[] {0,.4,.7,.1,1,.7,.4,0};
		return fit[x];
	}
	protected double companerosAlto( int x ){
		if (x >= 8)x = 7;
		//double[] fit = new double[] {0,0,0,0,.2,.4,.6,.8,1,.7,.5};
		double[] fit = new double[] {0,0,0,0,0,0,.2,.4,.6,.8,1};
		return fit[x];
	}

	//STAMINA
	protected double staminaBaja( double x , double maxSta){//  ¯¯¯¯¯¯\____
		double stam=(x*100)/maxSta;//porcentaje de stamina
		if(stam>=0 && stam<50){
			return 1;
		}else if(stam>=50 && stam<70){
			return (70-stam)/20;
		}else{
			return 0;
		}
	}
	protected double staminaAlta( double x, double maxSta){ // _____/¯¯¯¯¯
		double stam=(x*100)/maxSta;//porcentaje de stamina
		if(stam>=0 && stam<50){
			return 0;
		}else if(stam>=50 && stam<70){
			return (stam-50)/20;
		}else{
			return 1;
		}

	}

	//HP
	protected double hpBajo( double x , double maxHp){//  ¯¯¯¯¯¯\____
		double hp=(x*100)/maxHp;//porcentaje de stamina
		if(hp>=0 && hp<50){
			return 1;
		}else if(hp>=50 && hp<70){
			return (70-hp)/20;
		}else{
			return 0;
		}
	}
	protected double hpAlt( double x, double maxHp){ // _____/¯¯¯¯¯
		double hp=(x*100)/maxHp;//porcentaje de stamina
		if(hp>=0 && hp<50){
			return 0;
		}else if(hp>=50 && hp<70){
			return (hp-50)/20;
		}else{
			return 1;
		}
		
	}

   //ETAPAS DE VIDA  
	protected double juventud( double x ,double maxLifetime ){  //  ¯¯¯¯¯¯\____
		double life=(x*100)/maxLifetime;//porcentaje de stamina
		if(life>=0 && life<20){
			return 1;
		}else if(life>=20 && life<40){
			return (40-life)/30;
		}else{
			return 0;
		}
	}  
	protected double adultez( double x,double maxLifetime ){ //      ___/¯¯¯¯\___
		double life=(x*100)/maxLifetime;//porcentaje de stamina
		if(life>=0 && life<20){
			return 0;
		}else if(life>=20 && life<40){
			return (life-20)/20;
		}else if (life >=40 && life <60){
			return 1;
		}else if (life >=60 && life <80){
			return (80-life)/20;
		}else{
			return 0;
		}
	}

	/*
	 * 
	 *  lifetime += Time.deltaTime;

            if (lifetime < 240)
            {
                LifeState = LifeEnum.Joven;
            }
            else if (lifetime < 480)
            {
                LifeState = LifeEnum.Adulto;
            }
            else
            {
                LifeState = LifeEnum.Vejez;
            }*/

	protected double vejez( double x,double maxLifetime ){ // _____/¯¯¯¯¯
		double life=(x*100)/maxLifetime;//porcentaje de stamina
		if(life>=0 && life<60){
			return 0;
		}else if(life>=60 && life<80){
			return (life-60)/20;
		}else{
			return 1;
		}
	}
	//PRIORIDADES
	protected double correr( int x ){  //  ¯¯¯¯¯¯\____ 
		return run[x];
	}
	protected double reproducirse( int x ){ // _/¯\_
		return reproduce[x];
	}
	protected double comer( int x ){// _____/¯¯¯¯¯
		return eat[x];
	}
}
