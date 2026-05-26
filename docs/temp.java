import java.util.ArrayList;
import java.util.List;

class Naudotojas {
    private String vardas;
    private String pavarde;
    private String elPastas;
    private String slaptazodis;
    private PaskyrosBusena paskyrosBusena;
}

class Administratorius extends Naudotojas {
    private String administratoriausLygis;
}

class Kelione {
    private String pavadinimas;
    private String pradziosData;
    private String pabaigosData;
    private KelionesBusena kelionesBusena;

    private Marsrutas marsrutas;
    private ReikmenuSarasas reikmenuSarasas = new ReikmenuSarasas();
}

class Marsrutas {
    private String pavadinimas;
    private float atstumas;
    private String pradinisMiestas;
    private String galutinisMiestas;
    private String polyline;

    private List<LankytinasObjektas> lankytiniObjektai = new ArrayList<>();

    public void atnaujintiMarsrutoDuomenis(GoogleMapsApiClient googleMapsApiClient) {
    }
}

class ReikmenuSarasas {
    private String sukurimoData;
    private List<Daiktas> daiktai = new ArrayList<>();
}

class Daiktas {
    private String pavadinimas;
    private String tipas;
}

class LankytinasObjektas {
    private String pavadinimas;
    private String tipas;
    private String adresas;
    private boolean bilietas;
    private int darboValandos;
    private float reitingas;
    private String ilguma;
    private String platuma;
}

class GoogleMapsApiClient {
    private String apiRaktas;
    private String bazinisUrl;
}

enum PaskyrosBusena {
    AKTYVI,
    UZBLOKUOTA
}

enum KelionesBusena {
    IVYKUSI,
    SUPLANUOTA,
    PLANUOJAMA
}