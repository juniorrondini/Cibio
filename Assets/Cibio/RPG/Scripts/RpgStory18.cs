using UnityEngine;

namespace Cibio.RPG
{
    public static class RpgStory18
    {
        public const string Setting="BRASIL DA VUVUZELA";
        public const string Genre="Sátira política ficcional";
        public const string Mission="Brasil no Vermelho";
        public const string District="Distrito do Poder";
        public const string Home="Bairro do Trabalhador";
        public const string MenuDescription="Boletos, dívidas e medo. Reúna provas e enfrente o Companheiro.";
        public const string Premise="No Brasil da Vuvuzela, o salário acaba antes dos boletos. O Companheiro mantém o país preso a dívidas, juros abusivos e perseguição política. Há uma fraude na apuração do distrito. Reúna as provas, proteja o bairro e confronte quem sustenta esse sistema.";
        public const string Accept="Seu Bonero: o Companheiro sufocou o bairro com boletos, juros e medo. Reúna três provas no Distrito do Poder e confronte-o. [N] Dossiê.";
        public const string Reminder="Seu Bonero: os arquivos ficam ao norte. Afaste os agentes e recolha cada prova com E. O dossiê [N] guarda o que você descobrir.";
        public const string Delivered="Seu Bonero: o esquema foi exposto. Use a forma dourada e pressione C para evoluir. O Brasil da Vuvuzela ainda tem muita conta a acertar.";
        public static readonly string[] Stations={"Central de Cobrança","Arquivo da Perseguição","Central de Apuração"};
        public static readonly string[] Assets={"Cobranca18","Perseguicao18","Apuracao18"};
        public static readonly string[] Headlines={"A fábrica de dívidas","O arquivo do medo","A apuração roubada"};
        public static readonly string[] Themes={"BOLETOS • DÍVIDAS • JUROS ABUSIVOS","PERSEGUIÇÃO POLÍTICA","FRAUDE NA URNA"};
        public static readonly string[] Findings={
            "O livro-caixa registra juros sobre juros e boletos que continuam sendo cobrados depois de pagos. No bairro, trabalhar já não basta para sair da dívida.",
            "As listas relacionam críticas ao Companheiro a multas, intimidações e visitas de agentes. Os arquivos revelam como o medo mantém o bairro calado.",
            "O log da urna não coincide com o resultado divulgado pela central do Companheiro. A trilha recuperada expõe a fraude da apuração neste distrito fictício."};
        public static readonly string[] Pickup={
            "PROVA 1 • Livro-caixa: cobranças duplicadas e juros abusivos. [N] Ler dossiê.",
            "PROVA 2 • Lista de perseguição: o sistema usa multas e agentes contra os críticos. [N] Ler dossiê.",
            "PROVA 3 • Log da urna: a apuração do distrito foi adulterada. [N] Ler dossiê."};
        public static int Count(RpgProgress p){int n=0;for(int i=0;i<3;i++)if(Has(p,i))n++;return n;}
        public static bool Has(RpgProgress p,int id)=>id>=0&&id<3&&(p.silenced&(1<<id))!=0;
        public static string Objective(RpgProgress p)=>p.questStage==0?"Fale com Seu Bonero, ao sul da praça.":p.questStage==3?"Esquema exposto. A Forma Ascendente e a Palma estão disponíveis.":p.questStage==2?"Entregue as provas a Seu Bonero, ao sul da praça.":"Recolha três provas no Distrito do Poder e derrote o Companheiro.";
        public static string Progress(RpgProgress p)=>Count(p)+"/3 provas  ·  Companheiro "+(p.captainDefeated?"vencido":"ativo");
        public static string Context(RpgProgress p,int id)=>Has(p,id)?Stations[id]+" • prova recolhida":"E   Investigar "+Stations[id];
        public static string EnemyName(int kind)=>kind==5?"Bonero Companheiro":kind==4?"Companheiro":kind==3?"Agente da Perseguição":kind==2?"Pregoeiro do Sistema":kind==1?"Fiscal da Dívida":"Cobrador";
    }
}
