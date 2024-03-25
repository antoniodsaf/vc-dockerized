<template>
  <div>
    <div v-if="!configure?.value">
      <div class="relative z-0 flex">
        <input
          ref="inputElement"
          v-model.number="enteredQuantity"
          type="number"
          :disabled="disabled"
          :max="maxQty"
          :min="minQty"
          :class="{
            'z-10 border-[color:var(--color-danger)] focus:border-[color:var(--color-danger-hover)]': !!errorMessage,
          }"
          class="-mr-px h-9 w-full min-w-0 flex-1 appearance-none rounded-l rounded-r-none border border-gray-300 px-1 text-center text-base leading-9 outline-none focus:border-gray-400 lg:text-sm"
          @input="onInput"
          @keypress="onKeypress"
          @click="onClick"
          @blur="onBlur"
        />

        <VcButton
          class="w-28 !rounded-l-none"
          :variant="countInCart ? 'solid' : 'outline'"
          :loading="false"
          :disabled="disabled || !!errorMessage"
          :title="buttonText"
          size="sm"
          truncate
          @click="onChange"
        >
          {{ buttonText }}
        </VcButton>
      </div>
    </div>
    <div v-if="configure?.value">
      <VcButton
        class="w-48 !rounded-l-none"
        variant="solid"
        :loading="loading"
        title="Configure"
        size="lg"
        truncate
        @click="onConfigure"
      >
        Configure
      </VcButton>
    </div>

    <!-- Info hint -->
    <VcTooltip v-if="errorMessage" class="!block" :x-offset="28" placement="bottom-start" strategy="fixed">
      <template #trigger>
        <div class="line-clamp-1 pt-0.5 text-11 text-[color:var(--color-danger)]">
          {{ errorMessage }}
        </div>
      </template>

      <template #content>
        <div class="w-52 rounded-sm bg-white px-3.5 py-1.5 text-xs text-tooltip shadow-sm-x-y">
          {{ errorMessage }}
        </div>
      </template>
    </VcTooltip>

    <div v-else-if="reservedSpace" class="h-4"></div>
  </div>
</template>

<script setup lang="ts">
import { toTypedSchema } from "@vee-validate/yup";
import { clone } from "lodash";
import { useField } from "vee-validate";
import { computed, onMounted, onUnmounted, ref, shallowRef, watchEffect, type Ref } from "vue";
import { useI18n } from "vue-i18n";
import { useErrorsTranslator, useGoogleAnalytics } from "@/core/composables";
import { ValidationErrorObjectType } from "@/core/enums";
import { Logger } from "@/core/utilities";
import { useNotifications } from "@/shared/notification";
import { useQuantityValidationSchema } from "@/ui-kit/composables";
import { useCart } from "../composables/useCart";
import type { Product, LineItemType, VariationType } from "@/core/api/graphql/types";
import type { NamedValue } from "vue-i18n";
import { getCPQConfig } from "@/core/api/graphql";
import { usePopup } from "@/shared/popup";
import Modal from "./configure-modal.vue";

const emit = defineEmits<IEmits>();

const props = defineProps<IProps>();
const configure: Ref<Product["properties"][0] | null> = ref(null);
const cpqConfig: Ref<{
  cpqConfigure: {
    loginSuccess: boolean;
    url: string;
  };
} | null> = ref(null);
const { openPopup, closePopup } = usePopup();
watchEffect(() => {
  console.log(configure.value);
  configure.value = props.product.properties.find((e) => e.name === "Externally_Configurable_SKU") ?? null;
});
/* eslint-disable  @typescript-eslint/no-explicit-any */
const listener = ref((event: any) => {
  console.log("data", event.data);
  const key = event.message ? "message" : "data";
  const data = event[key];
  if (event.origin !== "https://objectedgeinc.bigmachines.com") {
    return;
  }
  const iframe = (document.getElementById("virto-poc-iframe") as HTMLIFrameElement)?.contentWindow;
  if (!data || /^error/i.test(data)) {
    /* eslint-disable  @typescript-eslint/no-explicit-any */
    const errorElement: any = [];
    /* eslint-disable  @typescript-eslint/no-explicit-any */
    const errorPayload: any = {};
    errorPayload.status = "ERROR";
    errorPayload.error = errorElement;
    /* eslint-disable  @typescript-eslint/no-explicit-any */
    const errorContent: any = {};
    errorContent.catalogRefId = "Data Error";
    errorContent.errorMsg = data || "Configuration data not available";
    errorPayload.error.push(errorContent);
    iframe?.postMessage(errorPayload, event.origin);
    return;
  }
  onAddToCart(JSON.parse(data));
  /* eslint-disable  @typescript-eslint/no-explicit-any */
  const errorData: any = [];
  /* eslint-disable  @typescript-eslint/no-explicit-any */
  const successData: any = {};
  successData.status = "SUCCESS";
  successData.error = errorData;
  iframe?.postMessage(successData, event.origin);
  window.removeEventListener("message", listener.value);
  console.log("::: listener removed");
  closePopup("poc");
});

const onConfigure = async () => {
  loading.value = true;
  let result = null;
  if (!cpqConfig.value) {
    result = await getCPQConfig({
      model: props.product.properties.find((e) => e.name === "Model")?.value,
      segment: props.product.properties.find((e) => e.name === "Product_Family")?.value,
      productLine: props.product.properties.find((e) => e.name === "Product_Line")?.value,
    });
  }
  window.addEventListener("message", listener.value);
  console.log("::: listener added", result);

  openPopup({
    component: Modal,
    props: {
      iframeURL: result?.cpqConfigure.url,
      listener: listener.value,
    },
    id: "poc",
  });
  loading.value = false;
};
const notifications = useNotifications();

interface IEmits {
  (event: "update:lineItem", lineItem: LineItemType): void;
}

interface IProps {
  product: Product | VariationType;
  reservedSpace?: boolean;
}

// Define max qty available to add
const MAX_VALUE = 999999999;

const minQuantity = computed(() => props.product.minQuantity);
const maxQuantity = computed(() => props.product.maxQuantity);

const { cart, addToCart, changeItemQuantity } = useCart();
const { t } = useI18n();
const ga = useGoogleAnalytics();
const { getTranslation } = useErrorsTranslator("validation_error");
const { quantitySchema } = useQuantityValidationSchema(minQuantity.value, maxQuantity.value);

const loading = ref(false);
const inputElement = shallowRef<HTMLInputElement>();

const countInCart = computed<number>(() => getLineItem(cart.value?.items)?.quantity || 0);
const minQty = computed<number>(() => minQuantity.value || 1);
const maxQty = computed<number>(() =>
  Math.min(props.product.availabilityData?.availableQuantity || MAX_VALUE, maxQuantity.value || MAX_VALUE),
);

const disabled = computed<boolean>(() => loading.value || !props.product.availabilityData?.isAvailable);

const buttonText = computed<string>(() =>
  countInCart.value ? t("common.buttons.update_cart") : t("common.buttons.add_to_cart"),
);

const rules = computed(() => toTypedSchema(quantitySchema.value));

const enteredQuantity = ref(!disabled.value ? countInCart.value || minQty.value : undefined);

const { errorMessage, validate, setValue } = useField("quantity", rules, { initialValue: enteredQuantity });

/**
 * Process button click to add/update cart line item.
 */
async function onChange() {
  const { valid } = await validate();

  if (!valid || disabled.value) {
    return;
  }

  loading.value = true;

  let lineItem = getLineItem(cart.value?.items);

  let updatedCart;

  const isAlreadyExistsInTheCart = !!lineItem;
  if (isAlreadyExistsInTheCart) {
    updatedCart = await changeItemQuantity(lineItem!.id, enteredQuantity.value || 0);
  } else {
    const inputQuantity = enteredQuantity.value || minQty.value;

    updatedCart = await addToCart(props.product.id!, inputQuantity);

    /**
     * Send Google Analytics event for an item added to cart.
     */
    ga.addItemToCart(props.product, inputQuantity);
  }

  lineItem = clone(getLineItem(updatedCart?.items));

  if (!lineItem) {
    Logger.error(onChange.name, 'The variable "lineItem" must be defined');
    notifications.error({
      text: t(
        isAlreadyExistsInTheCart
          ? "common.messages.fail_to_change_quantity_in_cart"
          : "common.messages.fail_add_product_to_cart",
        {
          reason: updatedCart.validationErrors
            ?.filter(
              (validationError) =>
                validationError.objectId === props.product.id &&
                validationError.objectType === ValidationErrorObjectType.CatalogProduct,
            )
            .map((el) => {
              return getTranslation({
                code: el.errorCode,
                parameters: el.errorParameters?.reduce((acc, err) => {
                  acc[err.key] = err.value;
                  return acc;
                }, {} as NamedValue),
                description: el.errorMessage,
              });
            })
            .join(" "),
        },
      ),
      duration: 4000,
      single: true,
    });
  } else {
    emit("update:lineItem", lineItem);
  }

  loading.value = false;
}
async function onAddToCart(data: ConfigurationDetails) {
  loading.value = true;
  if (!getLineItem(cart.value?.items)) {
    const inputQuantity = enteredQuantity.value || minQty.value;
    const { configXML, childItems, ...configurationData } = data;
    configXML;
    const updatedCart = await addToCart(props.product.id!, inputQuantity, [
      { name: "configurationId", value: data.configurationId },
      { name: "configurationData", value: JSON.stringify(configurationData) },
      { name: "childOptions", value: JSON.stringify(childItems) },
    ]);

    const lineItem = clone(getLineItem(updatedCart?.items));
    if (lineItem) {
      emit("update:lineItem", lineItem);
    }
  } else {
    alert("The configured product is already in cart.");
  }

  loading.value = false;
}

interface ConfigurationDetails {
  messageType: string;
  quantity: string;
  catalogRefId: string;
  externalPrice: string;
  price: string;
  currencyCode: string | null;
  configurationId: string;
  configXML: string;
  /* eslint-disable  @typescript-eslint/no-explicit-any */
  childItems: any;
}

function getLineItem(items?: LineItemType[]): LineItemType | undefined {
  return items?.find((item) => item.productId === props.product.id);
}

/**
 * Ignore non-numeric keys.
 */
function onKeypress(event: KeyboardEvent) {
  if (!/[0-9]/.test(event.key)) {
    event.preventDefault();
  }
}

/**
 * Limit max value.
 */
function onInput() {
  if (!enteredQuantity.value) {
    enteredQuantity.value = undefined;
  } else if (enteredQuantity.value > MAX_VALUE) {
    enteredQuantity.value = MAX_VALUE;
  } else {
    setValue(enteredQuantity.value);
  }
}

function onBlur() {
  if (!enteredQuantity.value || enteredQuantity.value < 1) {
    enteredQuantity.value = countInCart.value || minQty.value;
  }
}

/**
 * Select input value.
 */
function onClick() {
  inputElement.value!.select();
}
</script>
